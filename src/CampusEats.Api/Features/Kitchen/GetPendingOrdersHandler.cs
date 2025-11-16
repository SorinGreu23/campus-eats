using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Kitchen;

public class GetPendingOrdersHandler : IRequestHandler<GetPendingOrdersQuery, IResult>
{
    private readonly CampusDbContext _context;

    public GetPendingOrdersHandler(CampusDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(GetPendingOrdersQuery request, CancellationToken cancellationToken)
    {
        var pendingStatuses = new[] { OrderStatus.Pending, OrderStatus.Preparing }
            .Select(s => s.ToString())
            .ToArray();

        var orders = await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(oi => oi.MenuItem)
            .Where(o => pendingStatuses.Contains(o.Status ?? ""))
            .OrderBy(o => o.CreatedAt)
            .Select(o => new PendingOrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Status = o.Status,
                Total = o.Total,
                SpecialInstructions = o.SpecialInstructions,
                PickupTime = o.PickupTime,
                CreatedAt = o.CreatedAt,
                Items = o.Items.Select(oi => new PendingOrderItemDto
                {
                    Id = oi.Id,
                    MenuItemName = oi.MenuItem != null ? oi.MenuItem.Name : null,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Subtotal = oi.Subtotal,
                    SpecialInstructions = oi.SpecialInstructions
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        return Results.Ok(orders);
    }
}

