using CampusEats.Api.Data;
using CampusEats.Api.Features.Kitchen;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Orders.GetPendingOrders;

public class GetPendingOrdersHandler : IRequestHandler<GetPendingOrdersQuery, IResult>
{
    private readonly CampusDbContext _context;

    public GetPendingOrdersHandler(CampusDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(
        GetPendingOrdersQuery request,
        CancellationToken cancellationToken
    )
    {
        var pendingStatuses = new[] { OrderStatus.Pending, OrderStatus.Preparing }.Select(s =>
            s.ToString()
        );

        var orders = await _context
            .Orders.Where(o => pendingStatuses.Contains(o.Status))
            .OrderBy(o => o.CreatedAt)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.MenuItem)
            .Select(o => new PendingOrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Status = o.Status,
                Total = o.Total,
                PickupTime = o.PickupTime,
                CreatedAt = o.CreatedAt,
                Items = o.Items.Select(oi => new PendingOrderItemDto
                {
                    Id = oi.Id,
                    MenuItemName = oi.MenuItem!.Name,
                    MenuItemImageUrl = oi.MenuItem!.ImageUrl,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Subtotal = oi.Subtotal,
                    SpecialInstructions = oi.SpecialInstructions,
                }),
            })
            .ToListAsync(cancellationToken);

        return Results.Ok(orders);
    }
}
