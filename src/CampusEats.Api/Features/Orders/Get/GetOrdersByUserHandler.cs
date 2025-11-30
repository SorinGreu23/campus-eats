using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Orders.Get;

public class GetOrdersByUserHandler : IRequestHandler<GetOrdersByUserRequest, IResult>
{
    private readonly CampusDbContext _db;

    public GetOrdersByUserHandler(CampusDbContext db)
    {
        _db = db;
    }

    public async Task<IResult> Handle(GetOrdersByUserRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            return Results.BadRequest(new { error = "userId is required." });

        var orders = await _db.Orders
            .Where(o => o.UserId == request.UserId)
            .Include(o => o.Items)
                .ThenInclude(i => i.MenuItem)
            .OrderByDescending(o => o.Id)
            .ToListAsync(cancellationToken);

        if (orders == null || !orders.Any())
            return Results.NotFound(new { error = "No orders found for the specified user." });
        
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
                    i.MenuItem.Description
                },
                i.Quantity,
                i.UnitPrice,
                i.Subtotal,
                i.SpecialInstructions
            })
        });

        return Results.Ok(result);
    }
}

