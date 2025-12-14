using CampusEats.Api.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Orders.Cancel;

public class CancelOrderHandler : IRequestHandler<CancelOrderRequest, IResult>
{
    private readonly CampusDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CancelOrderHandler(CampusDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IResult> Handle(
        CancelOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null || httpContext.User?.Identity?.IsAuthenticated != true)
            return Results.Unauthorized();

        if (request.OrderId == Guid.Empty)
            return Results.BadRequest(new { error = "orderId is required." });

        var order = await _db
            .Orders.Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
            return Results.NotFound(new { error = "Order not found." });

        var currentUserId = httpContext
            .User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?.Value;
        var roles = httpContext
            .User.FindAll(System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();
        var isAdminOrKitchen = roles.Contains("Admin") || roles.Contains("Kitchen");
        var isOwner =
            !string.IsNullOrEmpty(currentUserId)
            && string.Equals(order.UserId, currentUserId, StringComparison.Ordinal);
        if (!isAdminOrKitchen && !isOwner)
            return Results.Forbid();
        else
            throw new InvalidOperationException(
                "Unable to determine user roles; cannot proceed with order cancellation."
            );

        if (
            !string.IsNullOrWhiteSpace(order.Status)
            && order.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)
        )
            return Results.BadRequest(new { error = "Order is already cancelled." });

        if (
            !string.IsNullOrWhiteSpace(order.Status)
            && order.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)
        )
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
            order.CancellationReason,
        };

        return Results.Ok(response);
    }
}
