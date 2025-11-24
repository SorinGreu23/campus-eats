using CampusEats.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.InventoryReports.Manage;

public record GetInventoryItemsQuery : IRequest<IResult>;

public record InventoryItemDto(Guid Id, string Name, string Unit, decimal CurrentQuantity, decimal MinimumQuantity, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

public class GetInventoryItemsHandler : IRequestHandler<GetInventoryItemsQuery, IResult>
{
    private readonly CampusDbContext _context;

    public GetInventoryItemsHandler(CampusDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(GetInventoryItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.InventoryItems
            .Select(i => new InventoryItemDto(
                i.Id,
                i.Name,
                i.Unit ?? string.Empty,
                i.CurrentQuantity,
                i.MinimumQuantity,
                i.CreatedAt,
                i.UpdatedAt))
            .ToListAsync(cancellationToken);

        return Results.Ok(items);
    }
}
