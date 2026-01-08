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

        // Load menu items from DB
        var menuItems = await _db
            .MenuItems.Where(m => menuItemIds.Contains(m.Id))
            .ToListAsync(cancellationToken);
        if (menuItems.Count != menuItemIds.Count)
            return Results.BadRequest(new { error = "One or more menu items were not found." });

        if (string.IsNullOrWhiteSpace(request.UserId))
            return Results.BadRequest(new { error = "userId is required." });

        // Only the owner (authenticated user) can create an order for their account
        var currentUserId = httpContext
            .User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?.Value;
        if (string.IsNullOrEmpty(currentUserId))
            return Results.Unauthorized();
        if (!string.Equals(currentUserId, request.UserId, StringComparison.Ordinal))
            return Results.Forbid();

        // Create order and compute totals
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = GenerateOrderNumber(),
            UserId = request.UserId,
            Status = "Pending",
            DeliveryInstructions = request.DeliveryInstructions,
            OrderType = request.OrderType ?? "Pickup",
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

        var tax = Math.Round(subtotal * TaxRate, 2);
        var discount = 0m; // no discount logic in this change
        var total = subtotal + tax - discount;

        order.Subtotal = subtotal;
        order.Tax = tax;
        order.Discount = discount;
        order.Total = total;

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);

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
}
