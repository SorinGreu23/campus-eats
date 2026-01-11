using CampusEats.Api.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Inventory.GetById;

public class GetInventoryItemHandler : IRequestHandler<GetInventoryItemRequest, IResult>
{
    private readonly CampusDbContext _context;

    public GetInventoryItemHandler(CampusDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(GetInventoryItemRequest request, CancellationToken cancellationToken)
    {
        var item = await _context.InventoryItems
            .Where(i => i.Id == request.Id)
            .Select(i => new
            {
                Item = new InventoryItemDto(
                    i.Id,
                    i.Name,
                    i.Unit,
                    i.CurrentQuantity,
                    i.MinimumQuantity,
                    i.CurrentQuantity <= i.MinimumQuantity,
                    i.CurrentQuantity <= 0,
                    i.UpdatedAt
                ),
                RecentTransactions = i.Transactions
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(10)
                    .Select(t => new InventoryTransactionDto(
                        t.Id,
                        t.InventoryItemId,
                        i.Name,
                        t.TransactionType,
                        t.Quantity,
                        t.Reason,
                        t.PerformedBy,
                        t.CreatedAt
                    ))
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (item == null)
        {
            return Results.NotFound($"Inventory item with ID '{request.Id}' was not found.");
        }

        return Results.Ok(new
        {
            item.Item,
            item.RecentTransactions
        });
    }
}
