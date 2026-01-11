using CampusEats.Api.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Orders.Pending;

public class GetPendingOrdersHandler : IRequestHandler<GetPendingOrdersRequest, IResult>
{
    private readonly CampusDbContext _db;

    public GetPendingOrdersHandler(CampusDbContext db)
    {
        _db = db;
    }

    public async Task<IResult> Handle(GetPendingOrdersRequest request, CancellationToken cancellationToken)
    {
        var orders = await _db.Orders
            .Where(o => o.Status != null && o.Status == "Pending")
            .Include(o => o.Items)
                .ThenInclude(i => i.MenuItem)
            .OrderBy(o => o.Id)
            .ToListAsync(cancellationToken);

        var result = orders.Select(o => new
        {
            o.Id,
            o.OrderNumber,
            o.Status,
            o.OrderType,
            o.Subtotal,
            o.Tax,
            o.Discount,
            o.Total,
            o.DeliveryInstructions,
            o.PickupTime,
            o.CompletedAt,
            o.CancelledAt,
            o.CancellationReason,
            Items = o.Items.Select(i => new
            {
                i.Id,
                i.MenuItemId,
                MenuItem = i.MenuItem == null ? null : new
                {
                    i.MenuItem.Id,
                    i.MenuItem.Name,
                    i.MenuItem.Price,
                    i.MenuItem.Description,
                    i.MenuItem.ImageUrl
                },
                i.Quantity,
                i.UnitPrice,
                i.Subtotal,
                i.SpecialInstructions
            })
        }).ToList();

        return Results.Ok(result);
    }
}
