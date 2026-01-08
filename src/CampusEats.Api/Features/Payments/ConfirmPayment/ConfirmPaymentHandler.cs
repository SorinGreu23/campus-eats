using CampusEats.Api.Common.Interfaces;
using CampusEats.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Payments.ConfirmPayment;

public class ConfirmPaymentHandler : IRequestHandler<ConfirmPaymentRequest, IResult>
{
    private readonly CampusDbContext _db;
    private readonly IStripePaymentService _stripeService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ConfirmPaymentHandler(
        CampusDbContext db,
        IStripePaymentService stripeService,
        IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _stripeService = stripeService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IResult> Handle(ConfirmPaymentRequest request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null || httpContext.User?.Identity?.IsAuthenticated != true)
            return Results.Unauthorized();

        var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        // Find the payment
        var payment = await _db.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

        if (payment is null)
            return Results.NotFound(new { error = "Payment not found." });

        // Verify the user owns this payment
        if (payment.UserId != userId)
            return Results.Forbid();

        // Check payment status from Stripe
        var isSuccessful = await _stripeService.ConfirmPaymentAsync(request.PaymentIntentId, cancellationToken);

        if (isSuccessful)
        {
            // Update payment status
            payment.Status = "succeeded";
            payment.TransactionId = request.PaymentIntentId;
            payment.UpdatedAt = DateTimeOffset.UtcNow;

            // Update order status to Paid if associated with an order
            if (payment.OrderId.HasValue)
            {
                var order = await _db.Orders.FindAsync(new object[] { payment.OrderId.Value }, cancellationToken);
                if (order != null)
                {
                    order.Status = "Paid";
                    order.UpdatedAt = DateTimeOffset.UtcNow;
                }
            }

            await _db.SaveChangesAsync(cancellationToken);

            return Results.Ok(new { message = "Payment confirmed successfully.", status = "succeeded" });
        }

        payment.Status = "failed";
        payment.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return Results.BadRequest(new { error = "Payment confirmation failed.", status = "failed" });
    }
}
