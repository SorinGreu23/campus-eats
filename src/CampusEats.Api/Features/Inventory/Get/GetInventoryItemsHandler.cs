using CampusEats.Api.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Inventory.Get;

public class GetInventoryItemsHandler : IRequestHandler<GetInventoryItemsRequest, IResult>
{
    private readonly CampusDbContext _context;

    public GetInventoryItemsHandler(CampusDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(GetInventoryItemsRequest request, CancellationToken cancellationToken)
    {
        var items = await _context.InventoryItems
            .OrderBy(i => i.Name)
            .Select(i => new InventoryItemDto(
                i.Id,
                i.Name,
                i.Unit,
                i.CurrentQuantity,
                i.MinimumQuantity,
                i.CurrentQuantity <= i.MinimumQuantity,
                i.CurrentQuantity <= 0,
                i.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        return Results.Ok(items);
    }
}
