using CampusEats.Api.Data;
using CampusEats.Api.Features.Kitchen;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Orders.UpdateStatus;

public class UpdateOrderStatusHandler : IRequestHandler<UpdateOrderStatusRequest, IResult>
{
    private readonly CampusDbContext _context;
    private readonly IValidator<UpdateOrderStatusRequest> _validator;

    public UpdateOrderStatusHandler(
        CampusDbContext context,
        IValidator<UpdateOrderStatusRequest> validator
    )
    {
        _context = context;
        _validator = validator;
    }

    public async Task<IResult> Handle(
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken
    )
    {
        var validationError = await ValidateRequestAsync(request, cancellationToken);
        if (validationError != null)
            return validationError;

        var order = await LoadOrderAsync(request.OrderId, cancellationToken);
        if (order == null)
            return Results.NotFound("Order not found.");

        var transitionError = ValidateStatusTransition(order, request.Status);
        if (transitionError != null)
            return transitionError;

        await UpdateOrderStatusAsync(order, request, cancellationToken);

        return Results.NoContent();
    }

    private async Task<IResult?> ValidateRequestAsync(UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
        }
        return null;
    }

    private async Task<Data.Entities.Order?> LoadOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    private static IResult? ValidateStatusTransition(Data.Entities.Order order, string newStatusString)
    {
        var currentStatus = Enum.Parse<OrderStatus>(order.Status!);

        if (!Enum.TryParse<OrderStatus>(newStatusString, out var newStatus))
        {
            return Results.BadRequest("Invalid order status value.");
        }

        if (!IsValidStatusTransition(currentStatus, newStatus))
        {
            return Results.BadRequest($"Invalid status transition from {currentStatus} to {newStatus}.");
        }

        return null;
    }

    private async Task UpdateOrderStatusAsync(Data.Entities.Order order, UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var currentStatus = Enum.Parse<OrderStatus>(order.Status!);
        var newStatus = Enum.Parse<OrderStatus>(request.Status);

        UpdateOrderTimestamps(order, newStatus);

        if (ShouldDeductInventory(currentStatus, request.Status))
        {
            await DeductInventoryAsync(request.OrderId, currentStatus, request.Status, cancellationToken);
        }

        order.Status = request.Status;
        order.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static void UpdateOrderTimestamps(Data.Entities.Order order, OrderStatus newStatus)
    {
        if (newStatus == OrderStatus.Completed)
        {
            order.CompletedAt = DateTimeOffset.UtcNow;
        }

        if (newStatus == OrderStatus.Cancelled)
        {
            order.CancelledAt = DateTimeOffset.UtcNow;
        }
    }

    private static bool ShouldDeductInventory(OrderStatus currentStatus, string newStatusString)
    {
        return (newStatusString is "Paid" or "Preparing" or "Ready" or "Completed") && 
               currentStatus == OrderStatus.Pending;
    }

    private async Task DeductInventoryAsync(Guid orderId, OrderStatus currentStatus, string newStatus, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[UpdateStatus] Processing order {orderId} - transitioning from {currentStatus} to {newStatus}");

        var orderWithItems = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (orderWithItems == null)
        {
            Console.WriteLine($"[UpdateStatus] WARNING: Order {orderId} not found with items!");
            return;
        }

        Console.WriteLine($"[UpdateStatus] Found order with {orderWithItems.Items.Count} items");

        foreach (var orderItem in orderWithItems.Items)
        {
            await DeductInventoryForOrderItemAsync(orderItem, cancellationToken);
        }
    }

    private async Task DeductInventoryForOrderItemAsync(Data.Entities.OrderItem orderItem, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[UpdateStatus] Processing order item: MenuItemId={orderItem.MenuItemId}, Quantity={orderItem.Quantity}");

        var ingredients = await _context.MenuItemIngredients
            .Where(mii => mii.MenuItemId == orderItem.MenuItemId)
            .ToListAsync(cancellationToken);

        Console.WriteLine($"[UpdateStatus] Found {ingredients.Count} ingredients for menu item {orderItem.MenuItemId}");

        foreach (var ingredient in ingredients)
        {
            await DeductIngredientFromInventoryAsync(ingredient, orderItem.Quantity, cancellationToken);
        }
    }

    private async Task DeductIngredientFromInventoryAsync(Data.Entities.MenuItemIngredient ingredient, int orderQuantity, CancellationToken cancellationToken)
    {
        var inventoryItem = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.Id == ingredient.InventoryItemId, cancellationToken);

        if (inventoryItem == null)
        {
            Console.WriteLine($"[UpdateStatus] WARNING: Inventory item {ingredient.InventoryItemId} not found!");
            return;
        }

        var quantityToDeduct = ingredient.QuantityRequired * orderQuantity;
        Console.WriteLine($"[UpdateStatus] Deducting {quantityToDeduct} {inventoryItem.Unit} of {inventoryItem.Name} (was {inventoryItem.CurrentQuantity})");
        
        inventoryItem.CurrentQuantity -= quantityToDeduct;
        inventoryItem.UpdatedAt = DateTimeOffset.UtcNow;
        
        Console.WriteLine($"[UpdateStatus] New quantity: {inventoryItem.CurrentQuantity}");
    }

    private static bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        // Allow cancellation from Pending, Preparing, or Ready states
        if (newStatus == OrderStatus.Cancelled)
        {
            return currentStatus is OrderStatus.Pending or OrderStatus.Preparing or OrderStatus.Ready;
        }

        // Standard forward progression
        return currentStatus switch
        {
            OrderStatus.Pending => newStatus == OrderStatus.Preparing,
            OrderStatus.Preparing => newStatus == OrderStatus.Ready,
            OrderStatus.Ready => newStatus == OrderStatus.Completed,
            _ => false
        };
    }
}
