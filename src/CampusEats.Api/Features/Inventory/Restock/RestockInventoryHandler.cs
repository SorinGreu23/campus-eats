using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Inventory.Restock;

public class RestockInventoryHandler : IRequestHandler<RestockInventoryRequest, IResult>
{
    private readonly CampusDbContext _context;
    private readonly IValidator<RestockInventoryRequest> _validator;

    public RestockInventoryHandler(CampusDbContext context, IValidator<RestockInventoryRequest> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<IResult> Handle(RestockInventoryRequest request, CancellationToken cancellationToken)
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

        var transaction = new InventoryTransaction
        {
            Id = Guid.NewGuid(),
            InventoryItemId = request.InventoryItemId,
            TransactionType = "Restock",
            Quantity = request.Quantity,
            Reason = request.Reason,
            PerformedBy = "chef", // TODO: Get from authenticated user context
            CreatedAt = DateTimeOffset.UtcNow
        };

        inventoryItem.CurrentQuantity += request.Quantity;
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
            transaction.Quantity,
            NewQuantity = inventoryItem.CurrentQuantity,
            transaction.Reason,
            transaction.PerformedBy,
            transaction.CreatedAt
        };

        return Results.Created($"/api/inventory/transactions/{response.Id}", response);
    }
}
