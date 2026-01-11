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
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var order = await _context.Orders.FirstOrDefaultAsync(
            o => o.Id == request.OrderId,
            cancellationToken
        );

        if (order == null)
        {
            return Results.NotFound("Order not found.");
        }

        var currentStatus = Enum.Parse<OrderStatus>(order.Status!);

        if (!Enum.TryParse<OrderStatus>(request.Status, out var newStatus))
        {
            return Results.BadRequest("Invalid order status value.");
        }

        if (!IsValidStatusTransition(currentStatus, newStatus))
        {
            return Results.BadRequest(
                $"Invalid status transition from {currentStatus} to {newStatus}."
            );
        }

        if (newStatus == OrderStatus.Completed)
        {
            order.CompletedAt = DateTimeOffset.UtcNow;
        }

        // Deduct inventory when order transitions to Paid, Preparing, Ready, or Completed
        // Only deduct if transitioning from Pending (to prevent double deduction)
        var shouldDeductInventory = (request.Status == "Paid" || 
                                     request.Status == "Preparing" || 
                                     request.Status == "Ready" || 
                                     request.Status == "Completed") && 
                                    currentStatus == OrderStatus.Pending;

        if (shouldDeductInventory)
        {
            Console.WriteLine($"[UpdateStatus] Processing order {request.OrderId} - transitioning from {currentStatus} to {request.Status}");
            
            var orderWithItems = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (orderWithItems != null)
            {
                Console.WriteLine($"[UpdateStatus] Found order with {orderWithItems.Items.Count} items");
                
                foreach (var orderItem in orderWithItems.Items)
                {
                    Console.WriteLine($"[UpdateStatus] Processing order item: MenuItemId={orderItem.MenuItemId}, Quantity={orderItem.Quantity}");
                    
                    // Get the ingredients needed for this menu item
                    var ingredients = await _context.MenuItemIngredients
                        .Where(mii => mii.MenuItemId == orderItem.MenuItemId)
                        .ToListAsync(cancellationToken);

                    Console.WriteLine($"[UpdateStatus] Found {ingredients.Count} ingredients for menu item {orderItem.MenuItemId}");

                    foreach (var ingredient in ingredients)
                    {
                        var inventoryItem = await _context.InventoryItems
                            .FirstOrDefaultAsync(i => i.Id == ingredient.InventoryItemId, cancellationToken);

                        if (inventoryItem != null)
                        {
                            var quantityToDeduct = ingredient.QuantityRequired * orderItem.Quantity;
                            Console.WriteLine($"[UpdateStatus] Deducting {quantityToDeduct} {inventoryItem.Unit} of {inventoryItem.Name} (was {inventoryItem.CurrentQuantity})");
                            inventoryItem.CurrentQuantity -= quantityToDeduct;
                            inventoryItem.UpdatedAt = DateTimeOffset.UtcNow;
                            Console.WriteLine($"[UpdateStatus] New quantity: {inventoryItem.CurrentQuantity}");
                        }
                        else
                        {
                            Console.WriteLine($"[UpdateStatus] WARNING: Inventory item {ingredient.InventoryItemId} not found!");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"[UpdateStatus] WARNING: Order {request.OrderId} not found with items!");
            }
        }
        else
        {
            Console.WriteLine($"[UpdateStatus] Skipping inventory deduction for order {request.OrderId} - current status: {currentStatus}, new status: {request.Status}");
        }

        order.Status = request.Status;
        order.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        return currentStatus switch
        {
            OrderStatus.Pending => newStatus == OrderStatus.Preparing,
            OrderStatus.Preparing => newStatus == OrderStatus.Ready,
            OrderStatus.Ready => newStatus == OrderStatus.Completed,
            _ => false,
        };
    }
}
