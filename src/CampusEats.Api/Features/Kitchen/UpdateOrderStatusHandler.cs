using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Kitchen;

public class UpdateOrderStatusHandler : IRequestHandler<UpdateOrderStatusCommand, IResult>
{
    private readonly CampusDbContext _context;
    private readonly IValidator<UpdateOrderStatusCommand> _validator;

    public UpdateOrderStatusHandler(CampusDbContext context, IValidator<UpdateOrderStatusCommand> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<IResult> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Results.BadRequest(new { errors });
        }

        var order = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.MenuItem)
            .ThenInclude(m => m.Ingredients)
            .ThenInclude(mi => mi.InventoryItem)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            return Results.NotFound(new { message = "Order not found." });
        }

        var currentStatus = Enum.TryParse<OrderStatus>(order.Status, out var parsedCurrent)
            ? parsedCurrent
            : OrderStatus.Pending;

        var newStatus = request.Status;

        if (!IsValidStatusTransition(currentStatus, newStatus))
        {
            return Results.BadRequest(new
            {
                message = $"Invalid status transition from '{currentStatus}' to '{newStatus}'.",
                validTransitions = "Pending → Preparing → Ready → Completed"
            });
        }

        // Deduct inventory when starting preparation
        if (newStatus == OrderStatus.Preparing)
        {
            var deductionResult = await DeductInventoryAsync(order, cancellationToken);
            if (!deductionResult.IsSuccess)
            {
                return Results.BadRequest(new { message = deductionResult.Error });
            }
        }

        order.Status = newStatus.ToString();
        order.UpdatedAt = DateTimeOffset.UtcNow;

        if (newStatus == OrderStatus.Completed)
        {
            order.CompletedAt = DateTimeOffset.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private async Task<(bool IsSuccess, string? Error)> DeductInventoryAsync(Order order, CancellationToken cancellationToken)
    {
        var requiredIngredients = new Dictionary<Guid, decimal>();

        // Calculate total required quantities
        foreach (var orderItem in order.Items)
        {
            if (orderItem.MenuItem?.Ingredients == null) continue;

            foreach (var ingredient in orderItem.MenuItem.Ingredients)
            {
                var totalRequired = ingredient.QuantityRequired * orderItem.Quantity;
                if (requiredIngredients.ContainsKey(ingredient.InventoryItemId))
                {
                    requiredIngredients[ingredient.InventoryItemId] += totalRequired;
                }
                else
                {
                    requiredIngredients[ingredient.InventoryItemId] = totalRequired;
                }
            }
        }

        // Check availability and deduct
        foreach (var (inventoryItemId, requiredQty) in requiredIngredients)
        {
            var inventoryItem = await _context.InventoryItems.FindAsync(new object[] { inventoryItemId }, cancellationToken);
            
            if (inventoryItem == null)
            {
                return (false, $"Inventory item {inventoryItemId} not found.");
            }

            if (inventoryItem.CurrentQuantity < requiredQty)
            {
                return (false, $"Insufficient inventory for '{inventoryItem.Name}'. Required: {requiredQty}, Available: {inventoryItem.CurrentQuantity}");
            }

            inventoryItem.CurrentQuantity -= requiredQty;
            inventoryItem.UpdatedAt = DateTimeOffset.UtcNow;

            _context.InventoryTransactions.Add(new InventoryTransaction
            {
                Id = Guid.NewGuid(),
                InventoryItemId = inventoryItemId,
                TransactionType = "OrderUsage",
                Quantity = -requiredQty,
                Reason = $"Used for Order {order.OrderNumber}",
                PerformedBy = null 
            });
        }

        return (true, null);
    }

    private static bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        return currentStatus switch
        {
            OrderStatus.Pending => newStatus == OrderStatus.Preparing,
            OrderStatus.Preparing => newStatus == OrderStatus.Ready,
            OrderStatus.Ready => newStatus == OrderStatus.Completed,
            _ => false
        };
    }
}

