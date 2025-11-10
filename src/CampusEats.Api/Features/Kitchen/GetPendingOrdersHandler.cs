using CampusEats.Api.Common.Models;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Kitchen;

public class GetPendingOrdersHandler : IRequestHandler<GetPendingOrdersQuery, Result<List<PendingOrderDto>>>
{
    private readonly CampusDbContext _context;

    public GetPendingOrdersHandler(CampusDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<PendingOrderDto>>> Handle(GetPendingOrdersQuery request, CancellationToken cancellationToken)
    {
        var pendingStatuses = new[] { "Pending", "Preparing" };

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

        return Result<List<PendingOrderDto>>.Success(orders);
    }
}

