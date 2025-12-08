using CampusEats.Api.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Orders.Complete;

public class CompleteOrderHandler : IRequestHandler<CompleteOrderRequest, IResult>
{
    private readonly CampusDbContext _db;

    public CompleteOrderHandler(CampusDbContext db)
    {
        _db = db;
    }

    public async Task<IResult> Handle(CompleteOrderRequest request, CancellationToken cancellationToken)
    {
        if (request.OrderId == Guid.Empty)
            return Results.BadRequest(new { error = "orderId is required." });

        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
            return Results.NotFound(new { error = "Order not found." });

        if (!string.IsNullOrWhiteSpace(order.Status) && order.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            return Results.BadRequest(new { error = "Cancelled orders cannot be completed." });

        if (!string.IsNullOrWhiteSpace(order.Status) && order.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            return Results.BadRequest(new { error = "Order is already completed." });

        order.Status = "Completed";
        order.CompletedAt = DateTimeOffset.UtcNow;

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
            order.CompletedAt
        };

        return Results.Ok(response);
    }
}
