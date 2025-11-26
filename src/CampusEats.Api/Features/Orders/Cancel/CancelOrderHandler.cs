using CampusEats.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Orders.Cancel;

public class CancelOrderHandler : IRequestHandler<CancelOrderRequest, IResult>
{
    private readonly CampusDbContext _db;

    public CancelOrderHandler(CampusDbContext db)
    {
        _db = db;
    }

    public async Task<IResult> Handle(CancelOrderRequest request, CancellationToken cancellationToken)
    {
        if (request.OrderId == Guid.Empty)
            return Results.BadRequest(new { error = "orderId is required." });

        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
            return Results.NotFound(new { error = "Order not found." });
        
        if (!string.IsNullOrWhiteSpace(order.Status) && order.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            return Results.BadRequest(new { error = "Order is already cancelled." });

        if (!string.IsNullOrWhiteSpace(order.Status) && order.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            return Results.BadRequest(new { error = "Completed orders cannot be cancelled." });

        order.Status = "Cancelled";
        order.CancelledAt = DateTimeOffset.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.Reason))
            order.CancellationReason = request.Reason;

        await _db.SaveChangesAsync(cancellationToken);

        var response = new
        {
            order.Id,
            order.OrderNumber,
            order.Status,
            order.Subtotal,
            order.Tax,
            order.Discount,
            order.Total,
            order.CancelledAt,
            order.CancellationReason
        };

        return Results.Ok(response);
    }
}

