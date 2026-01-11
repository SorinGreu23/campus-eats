using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using MediatR;
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
        var validationResult = await ValidateOrderRequestAsync(request, cancellationToken);
        if (validationResult.Error != null)
            return validationResult.Error;

        var order = CreateOrder(request);
        var subtotal = AddOrderItems(order, request.Items!, validationResult.MenuItems!);
        CalculateOrderTotals(order, subtotal, validationResult.RewardDiscount);

        await SaveOrderAndAwardPointsAsync(order, request.UserId!, cancellationToken);

        return CreateOrderResponse(order);
    }

    private async Task<(IResult? Error, List<MenuItem>? MenuItems, decimal RewardDiscount)> ValidateOrderRequestAsync(
        CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var authResult = ValidateAuthentication(request);
        if (authResult != null)
            return (authResult, null, 0m);

        var itemsValidation = ValidateRequestItems(request);
        if (itemsValidation != null)
            return (itemsValidation, null, 0m);

        var menuItemIds = request.Items!
            .Where(i => i.MenuItemId.HasValue)
            .Select(i => i.MenuItemId!.Value)
            .ToList();

        var menuItems = await LoadMenuItemsAsync(menuItemIds, cancellationToken);
        if (menuItems.Count != menuItemIds.Count)
            return (Results.BadRequest(new { error = "One or more menu items were not found." }), null, 0m);

        var stockValidation = ValidateStockAvailability(request.Items!, menuItems);
        if (stockValidation != null)
            return (stockValidation, null, 0m);

        var (rewardResult, rewardDiscount, appliedReward) = await ValidateAndApplyLoyaltyRewardAsync(
            request, cancellationToken);
        if (rewardResult != null)
            return (rewardResult, null, 0m);

        var subtotalForValidation = CalculateSubtotal(request.Items!, menuItems);
        var minOrderValidation = ValidateMinimumOrderAmount(appliedReward, subtotalForValidation);
        if (minOrderValidation != null)
            return (minOrderValidation, null, 0m);

        return (null, menuItems, rewardDiscount);
    }

    private static decimal CalculateSubtotal(ICollection<CreateOrderItemRequest> items, List<MenuItem> menuItems)
    {
        return items
            .Where(i => i.MenuItemId.HasValue)
            .Sum(i =>
            {
                var menuItem = menuItems.First(m => m.Id == i.MenuItemId!.Value);
                var quantity = Math.Max(1, i.Quantity);
                return menuItem.Price * quantity;
            });
    }

    private static IResult? ValidateMinimumOrderAmount(LoyaltyReward? reward, decimal subtotal)
    {
        if (reward?.MinimumOrderAmount.HasValue == true && subtotal < reward.MinimumOrderAmount.Value)
        {
            return Results.BadRequest(new
            {
                error = $"This reward requires a minimum order of {reward.MinimumOrderAmount.Value:F2} RON (before tax). Your current subtotal is {subtotal:F2} RON."
            });
        }
        return null;
    }

    private async Task SaveOrderAndAwardPointsAsync(Order order, string userId, CancellationToken cancellationToken)
    {
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);
        await AwardLoyaltyPointsAsync(userId, order.Total, cancellationToken);
    }

    private static IResult CreateOrderResponse(Order order)
    {
        return Results.Created($"/orders/{order.Id}", new
        {
            order.Id,
            order.OrderNumber,
            order.Status,
            order.Subtotal,
            order.Tax,
            order.Discount,
            order.Total,
        });
    }

    private IResult? ValidateAuthentication(CreateOrderRequest request)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
            return Results.Unauthorized();

        if (string.IsNullOrWhiteSpace(request.UserId))
            return Results.BadRequest(new { error = "userId is required." });

        var currentUserId = httpContext.User
            .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(currentUserId))
            return Results.Unauthorized();
        
        if (!string.Equals(currentUserId, request.UserId, StringComparison.Ordinal))
            return Results.Forbid();

        return null;
    }

    private static IResult? ValidateRequestItems(CreateOrderRequest request)
    {
        if (request.Items == null || request.Items.Count == 0)
            return Results.BadRequest(new { error = "Order must contain at least one item." });

        var hasValidItems = request.Items.Any(i => i.MenuItemId.HasValue);
        if (!hasValidItems)
            return Results.BadRequest(new { error = "Invalid items. Each item must reference a MenuItemId." });

        return null;
    }

    private async Task<List<MenuItem>> LoadMenuItemsAsync(List<Guid> menuItemIds, CancellationToken cancellationToken)
    {
        return await _db.MenuItems
            .Include(m => m.Ingredients)
                .ThenInclude(mi => mi.InventoryItem)
            .Where(m => menuItemIds.Contains(m.Id))
            .ToListAsync(cancellationToken);
    }

    private static IResult? ValidateStockAvailability(ICollection<CreateOrderItemRequest> items, List<MenuItem> menuItems)
    {
        var stockErrors = new List<string>();
        
        foreach (var itemReq in items.Where(i => i.MenuItemId.HasValue))
        {
            var menuItem = menuItems.First(m => m.Id == itemReq.MenuItemId!.Value);
            var requestedQuantity = Math.Max(1, itemReq.Quantity);

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

        if (stockErrors.Count > 0)
        {
            return Results.BadRequest(new
            {
                error = "Insufficient stock for one or more items.",
                details = stockErrors
            });
        }

        return null;
    }

    private async Task<(IResult? Result, decimal Discount, LoyaltyReward? Reward)> ValidateAndApplyLoyaltyRewardAsync(
        CreateOrderRequest request, CancellationToken cancellationToken)
    {
        if (!request.LoyaltyRewardId.HasValue)
            return (null, 0m, null);

        var reward = await _db.LoyaltyRewards
            .FirstOrDefaultAsync(r => r.Id == request.LoyaltyRewardId.Value, cancellationToken);

        if (reward == null)
            return (Results.BadRequest(new { error = "Loyalty reward not found." }), 0m, null);

        var rewardValidation = ValidateRewardAvailability(reward);
        if (rewardValidation != null)
            return (rewardValidation, 0m, null);

        var loyaltyAccount = await _db.LoyaltyAccounts
            .FirstOrDefaultAsync(la => la.UserId == request.UserId, cancellationToken);

        if (loyaltyAccount == null)
            return (Results.BadRequest(new { error = "Loyalty account not found." }), 0m, null);

        var tierValidation = ValidateTierRequirement(reward, loyaltyAccount);
        if (tierValidation != null)
            return (tierValidation, 0m, null);

        var claimResult = await ProcessRewardClaimAsync(reward, loyaltyAccount, cancellationToken);
        if (claimResult != null)
            return (claimResult, 0m, null);

        return (null, reward.DiscountValue ?? 0m, reward);
    }

    private static IResult? ValidateRewardAvailability(LoyaltyReward reward)
    {
        if (!reward.IsActive)
            return Results.BadRequest(new { error = "This reward is not currently active." });

        var now = DateTimeOffset.UtcNow;
        if (reward.ValidFrom.HasValue && reward.ValidFrom > now)
            return Results.BadRequest(new { error = "This reward is not yet valid." });

        if (reward.ValidUntil.HasValue && reward.ValidUntil < now)
            return Results.BadRequest(new { error = "This reward has expired." });

        return null;
    }

    private static IResult? ValidateTierRequirement(LoyaltyReward reward, LoyaltyAccount account)
    {
        if (string.IsNullOrEmpty(reward.MinimumTier))
            return null;

        var userTierRank = GetTierRank(account.Tier ?? "Bronze");
        var requiredTierRank = GetTierRank(reward.MinimumTier);

        if (userTierRank < requiredTierRank)
        {
            return Results.BadRequest(new
            {
                error = $"This reward requires {reward.MinimumTier} tier or higher. Your current tier is {account.Tier ?? "Bronze"}."
            });
        }

        return null;
    }

    private async Task<IResult?> ProcessRewardClaimAsync(
        LoyaltyReward reward, LoyaltyAccount account, CancellationToken cancellationToken)
    {
        var existingClaim = await _db.LoyaltyClaims
            .FirstOrDefaultAsync(c =>
                c.LoyaltyAccountId == account.Id &&
                c.RewardId == reward.Id &&
                c.Notes != "Used",
                cancellationToken);

        if (account.PointsBalance < reward.PointsCost)
        {
            return Results.BadRequest(new
            {
                error = $"Insufficient points. You need {reward.PointsCost} points but only have {account.PointsBalance}."
            });
        }

        account.PointsBalance -= reward.PointsCost;

        if (existingClaim == null)
        {
            existingClaim = new LoyaltyClaim
            {
                Id = Guid.NewGuid(),
                LoyaltyAccountId = account.Id,
                RewardId = reward.Id,
                ClaimedAt = DateTimeOffset.UtcNow,
                Notes = "Used",
            };
            _db.LoyaltyClaims.Add(existingClaim);
        }
        else
        {
            existingClaim.Notes = "Used";
        }

        return null;
    }

    private static Order CreateOrder(CreateOrderRequest request)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = GenerateOrderNumber(),
            UserId = request.UserId!,
            Status = "Pending",
            DeliveryInstructions = request.DeliveryInstructions,
            OrderType = request.OrderType ?? "Pickup",
            LoyaltyRewardId = request.LoyaltyRewardId,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        if (order.OrderType.Equals("Delivery", StringComparison.OrdinalIgnoreCase))
        {
            order.PickupTime = null;
        }

        return order;
    }

    private decimal AddOrderItems(Order order, ICollection<CreateOrderItemRequest> items, List<MenuItem> menuItems)
    {
        decimal subtotal = 0m;

        foreach (var itemReq in items.Where(i => i.MenuItemId.HasValue))
        {
            var menuItem = menuItems.First(m => m.Id == itemReq.MenuItemId!.Value);
            var quantity = Math.Max(1, itemReq.Quantity);
            var lineSubtotal = menuItem.Price * quantity;

            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                MenuItemId = menuItem.Id,
                Quantity = quantity,
                UnitPrice = menuItem.Price,
                Subtotal = lineSubtotal,
                SpecialInstructions = itemReq.SpecialInstructions,
            };

            order.Items.Add(orderItem);
            _db.OrderItems.Add(orderItem);
            subtotal += lineSubtotal;
        }

        return subtotal;
    }

    private static void CalculateOrderTotals(Order order, decimal subtotal, decimal discount)
    {
        var tax = Math.Round(subtotal * TaxRate, 2);
        order.Subtotal = subtotal;
        order.Tax = tax;
        order.Discount = discount;
        order.Total = Math.Max(0, subtotal + tax - discount);
    }

    private async Task AwardLoyaltyPointsAsync(string userId, decimal orderTotal, CancellationToken cancellationToken)
    {
        var account = await _db.LoyaltyAccounts
            .FirstOrDefaultAsync(la => la.UserId == userId, cancellationToken);

        if (account == null)
            return;

        var pointsEarned = (int)Math.Floor(orderTotal);
        account.PointsBalance += pointsEarned;
        account.LifetimePoints += pointsEarned;
        account.Tier = CalculateTier(account.LifetimePoints);

        await _db.SaveChangesAsync(cancellationToken);
    }

    private static string CalculateTier(int lifetimePoints)
    {
        return lifetimePoints switch
        {
            >= 10000 => "Platinum",
            >= 5000 => "Gold",
            >= 2000 => "Silver",
            _ => "Bronze"
        };
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
