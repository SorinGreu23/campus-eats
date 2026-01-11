using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Inventory.Use;

public class UseInventoryHandler : IRequestHandler<UseInventoryRequest, IResult>
{
    private readonly CampusDbContext _context;
    private readonly IValidator<UseInventoryRequest> _validator;

    public UseInventoryHandler(CampusDbContext context, IValidator<UseInventoryRequest> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<IResult> Handle(UseInventoryRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );
            
            return Results.BadRequest(new { errors });
        }

        var inventoryItem = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.Id == request.InventoryItemId, cancellationToken);

        if (inventoryItem == null)
        {
            return Results.NotFound($"Inventory item with ID '{request.InventoryItemId}' was not found.");
        }

        if (inventoryItem.IsOutOfStock || inventoryItem.CurrentQuantity <= 0)
        {
            return Results.BadRequest(new
            {
                error = $"Cannot use '{inventoryItem.Name}' - item is out of stock.",
                itemId = inventoryItem.Id,
                itemName = inventoryItem.Name,
                currentQuantity = inventoryItem.CurrentQuantity
            });
        }

        if (inventoryItem.CurrentQuantity < request.Quantity)
        {
            return Results.BadRequest(new
            {
                error = $"Insufficient quantity of '{inventoryItem.Name}'. Available: {inventoryItem.CurrentQuantity} {inventoryItem.Unit}, Requested: {request.Quantity} {inventoryItem.Unit}",
                itemId = inventoryItem.Id,
                itemName = inventoryItem.Name,
                available = inventoryItem.CurrentQuantity,
                requested = request.Quantity,
                unit = inventoryItem.Unit
            });
        }

        var transaction = new InventoryTransaction
        {
            Id = Guid.NewGuid(),
            InventoryItemId = request.InventoryItemId,
            TransactionType = "Use",
            Quantity = -request.Quantity, // Negative for usage
            Reason = request.Reason,
            PerformedBy = "chef", // TODO: Get from authenticated user context
            CreatedAt = DateTimeOffset.UtcNow
        };

        inventoryItem.CurrentQuantity -= request.Quantity;
        inventoryItem.IsOutOfStock = inventoryItem.CurrentQuantity <= 0;
        inventoryItem.UpdatedAt = DateTimeOffset.UtcNow;

        _context.InventoryTransactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new
        {
            transaction.Id,
            transaction.InventoryItemId,
            ItemName = inventoryItem.Name,
            transaction.TransactionType,
            UsedQuantity = request.Quantity,
            RemainingQuantity = inventoryItem.CurrentQuantity,
            IsOutOfStock = inventoryItem.IsOutOfStock,
            IsLowStock = inventoryItem.CurrentQuantity <= inventoryItem.MinimumQuantity,
            transaction.Reason,
            transaction.PerformedBy,
            transaction.CreatedAt
        };

        return Results.Created($"/api/inventory/transactions/{response.Id}", response);
    }
}
