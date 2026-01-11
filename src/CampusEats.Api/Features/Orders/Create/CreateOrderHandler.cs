using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Orders.Create;

public class CreateOrderHandler : IRequestHandler<CreateOrderRequest, IResult>
{
    private readonly CampusDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const decimal TaxRate = 0.21m; //consider moving to config

    public CreateOrderHandler(CampusDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IResult> Handle(
        CreateOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null || httpContext.User?.Identity?.IsAuthenticated != true)
            return Results.Unauthorized();

        if (request.Items == null || !request.Items.Any())
            return Results.BadRequest(new { error = "Order must contain at least one item." });

        // Collect menu item ids
        var menuItemIds = request
            .Items.Where(i => i.MenuItemId.HasValue)
            .Select(i => i.MenuItemId!.Value)
            .ToList();
        if (!menuItemIds.Any())
            return Results.BadRequest(
                new { error = "Invalid items. Each item must reference a MenuItemId." }
            );

        // Load menu items from DB with ingredients
        var menuItems = await _db
            .MenuItems
            .Include(m => m.Ingredients)
                .ThenInclude(mi => mi.InventoryItem)
            .Where(m => menuItemIds.Contains(m.Id))
            .ToListAsync(cancellationToken);
        if (menuItems.Count != menuItemIds.Count)
            return Results.BadRequest(new { error = "One or more menu items were not found." });

        if (string.IsNullOrWhiteSpace(request.UserId))
            return Results.BadRequest(new { error = "userId is required." });

        // Validate stock availability for all items before creating the order
        var stockErrors = new List<string>();
        foreach (var itemReq in request.Items)
        {
            if (!itemReq.MenuItemId.HasValue)
                continue;
            
            var menuItem = menuItems.First(m => m.Id == itemReq.MenuItemId.Value);
            var requestedQuantity = Math.Max(1, itemReq.Quantity);
            
            // Check if menu item has ingredients
            if (menuItem.Ingredients.Any())
            {
                foreach (var ingredient in menuItem.Ingredients)
                {
                    var requiredQuantity = ingredient.QuantityRequired * requestedQuantity;
                    var availableQuantity = ingredient.InventoryItem.CurrentQuantity;
                    
                    if (availableQuantity < requiredQuantity)
                    {
                        stockErrors.Add(
                            $"Insufficient stock for '{menuItem.Name}': " +
                            $"requires {requiredQuantity} {ingredient.InventoryItem.Unit} of {ingredient.InventoryItem.Name}, " +
                            $"but only {availableQuantity} {ingredient.InventoryItem.Unit} available."
                        );
                    }
                }
            }
        }
        
        if (stockErrors.Any())
        {
            return Results.BadRequest(new 
            { 
                error = "Insufficient stock for one or more items.",
                details = stockErrors
            });
        }

        // Only the owner (authenticated user) can create an order for their account
        var currentUserId = httpContext
            .User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?.Value;
        if (string.IsNullOrEmpty(currentUserId))
            return Results.Unauthorized();
        if (!string.Equals(currentUserId, request.UserId, StringComparison.Ordinal))
            return Results.Forbid();

        // Validate and apply loyalty reward if provided
        decimal rewardDiscount = 0m;
        LoyaltyReward? appliedReward = null;

        if (request.LoyaltyRewardId.HasValue)
        {
            var reward = await _db
                .LoyaltyRewards.FirstOrDefaultAsync(
                    r => r.Id == request.LoyaltyRewardId.Value,
                    cancellationToken
                );

            if (reward == null)
                return Results.BadRequest(new { error = "Loyalty reward not found." });

            if (!reward.IsActive)
                return Results.BadRequest(new { error = "This reward is not currently active." });

            // Check validity dates
            var now = DateTimeOffset.UtcNow;
            if (reward.ValidFrom.HasValue && reward.ValidFrom > now)
                return Results.BadRequest(new { error = "This reward is not yet valid." });

            if (reward.ValidUntil.HasValue && reward.ValidUntil < now)
                return Results.BadRequest(new { error = "This reward has expired." });

            // Get user's loyalty account and verify they have claimed this reward
            var loyaltyAccount = await _db
                .LoyaltyAccounts.FirstOrDefaultAsync(
                    la => la.UserId == request.UserId,
                    cancellationToken
                );

            if (loyaltyAccount == null)
                return Results.BadRequest(new { error = "Loyalty account not found." });

            // Check tier requirement
            if (!string.IsNullOrEmpty(reward.MinimumTier))
            {
                var userTierRank = GetTierRank(loyaltyAccount.Tier ?? "Bronze");
                var requiredTierRank = GetTierRank(reward.MinimumTier);

                if (userTierRank < requiredTierRank)
                {
                    return Results.BadRequest(
                        new
                        {
                            error =
                                $"This reward requires {reward.MinimumTier} tier or higher. Your current tier is {loyaltyAccount.Tier ?? "Bronze"}."
                        }
                    );
                }
            }

            // Verify user has claimed this reward (has enough points and hasn't used it yet)
            var existingClaim = await _db
                .LoyaltyClaims.FirstOrDefaultAsync(
                    c =>
                        c.LoyaltyAccountId == loyaltyAccount.Id
                        && c.RewardId == reward.Id
                        && c.Notes != "Used",
                    cancellationToken
                );

            if (existingClaim == null)
            {
                // User hasn't claimed this reward yet, check if they have enough points
                if (loyaltyAccount.PointsBalance < reward.PointsCost)
                {
                    return Results.BadRequest(
                        new
                        {
                            error =
                                $"Insufficient points. You need {reward.PointsCost} points but only have {loyaltyAccount.PointsBalance}."
                        }
                    );
                }

                // Auto-claim the reward and deduct points
                loyaltyAccount.PointsBalance -= reward.PointsCost;
                var newClaim = new LoyaltyClaim
                {
                    Id = Guid.NewGuid(),
                    LoyaltyAccountId = loyaltyAccount.Id,
                    RewardId = reward.Id,
                    ClaimedAt = DateTimeOffset.UtcNow,
                    Notes = "Auto-claimed at checkout",
                };
                _db.LoyaltyClaims.Add(newClaim);
                existingClaim = newClaim;
            }
            else
            {
                // User has already claimed this reward, now deduct points when using it
                if (loyaltyAccount.PointsBalance < reward.PointsCost)
                {
                    return Results.BadRequest(
                        new
                        {
                            error =
                                $"Insufficient points. You need {reward.PointsCost} points but only have {loyaltyAccount.PointsBalance}."
                        }
                    );
                }
                loyaltyAccount.PointsBalance -= reward.PointsCost;
            }

            // Mark claim as used
            existingClaim.Notes = "Used";

            appliedReward = reward;
            rewardDiscount = reward.DiscountValue ?? 0m;
        }

        // Create order and compute totals
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = GenerateOrderNumber(),
            UserId = request.UserId,
            Status = "Pending",
            DeliveryInstructions = request.DeliveryInstructions,
            OrderType = request.OrderType ?? "Pickup",
            LoyaltyRewardId = request.LoyaltyRewardId,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        // If the client specified Delivery, ensure PickupTime is null
        if (
            !string.IsNullOrWhiteSpace(order.OrderType)
            && order.OrderType.Equals("Delivery", StringComparison.OrdinalIgnoreCase)
        )
        {
            order.PickupTime = null;
        }

        decimal subtotal = 0m;

        foreach (var itemReq in request.Items)
        {
            if (!itemReq.MenuItemId.HasValue)
                continue;
            var menuItem = menuItems.First(m => m.Id == itemReq.MenuItemId.Value);
            var unitPrice = menuItem.Price;
            var quantity = Math.Max(1, itemReq.Quantity);
            var lineSubtotal = unitPrice * quantity;

            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                MenuItemId = menuItem.Id,
                Quantity = quantity,
                UnitPrice = unitPrice,
                Subtotal = lineSubtotal,
                SpecialInstructions = itemReq.SpecialInstructions,
            };

            order.Items.Add(orderItem);
            _db.OrderItems.Add(orderItem);

            subtotal += lineSubtotal;
        }

        // Validate minimum order amount for reward
        if (appliedReward != null && appliedReward.MinimumOrderAmount.HasValue)
        {
            if (subtotal < appliedReward.MinimumOrderAmount.Value)
            {
                return Results.BadRequest(
                    new
                    {
                        error = $"This reward requires a minimum order of {appliedReward.MinimumOrderAmount.Value:F2} RON (before tax). Your current subtotal is {subtotal:F2} RON."
                    }
                );
            }
        }

        var tax = Math.Round(subtotal * TaxRate, 2);
        var discount = rewardDiscount;
        var total = Math.Max(0, subtotal + tax - discount);

        order.Subtotal = subtotal;
        order.Tax = tax;
        order.Discount = discount;
        order.Total = total;

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);

        // Award loyalty points: 1 RON spent = 1 point
        var userLoyaltyAccount = await _db
            .LoyaltyAccounts.FirstOrDefaultAsync(
                la => la.UserId == request.UserId,
                cancellationToken
            );

        if (userLoyaltyAccount != null)
        {
            var pointsEarned = (int)Math.Floor(total); // 1 RON = 1 point
            userLoyaltyAccount.PointsBalance += pointsEarned;
            userLoyaltyAccount.LifetimePoints += pointsEarned;

            // Update tier based on lifetime points
            if (userLoyaltyAccount.LifetimePoints >= 10000)
                userLoyaltyAccount.Tier = "Platinum";
            else if (userLoyaltyAccount.LifetimePoints >= 5000)
                userLoyaltyAccount.Tier = "Gold";
            else if (userLoyaltyAccount.LifetimePoints >= 2000)
                userLoyaltyAccount.Tier = "Silver";
            else
                userLoyaltyAccount.Tier = "Bronze";

            // Note: LoyaltyTransactions table doesn't exist yet, skipping transaction record
            // TODO: Add transaction tracking when table is created

            await _db.SaveChangesAsync(cancellationToken);
        }

        var response = new
        {
            order.Id,
            order.OrderNumber,
            order.Status,
            order.Subtotal,
            order.Tax,
            order.Discount,
            order.Total,
        };

        return Results.Created($"/orders/{order.Id}", response);
    }

    private static string GenerateOrderNumber()
    {
        var timePart = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var shortGuid = Guid.NewGuid().ToString().Split('-')[0];
        return $"ORD-{timePart}-{shortGuid}";
    }

    private static int GetTierRank(string tier)
    {
        return tier.ToLower() switch
        {
            "bronze" => 1,
            "silver" => 2,
            "gold" => 3,
            "platinum" => 4,
            _ => 1,
        };
    }
}
