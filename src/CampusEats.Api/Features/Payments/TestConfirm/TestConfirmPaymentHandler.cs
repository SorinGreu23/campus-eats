using CampusEats.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Payments.TestConfirm;

public class TestConfirmPaymentHandler : IRequestHandler<TestConfirmPaymentRequest, IResult>
{
    private readonly CampusDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TestConfirmPaymentHandler(
        CampusDbContext db,
        IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IResult> Handle(TestConfirmPaymentRequest request, CancellationToken cancellationToken)
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

        // Simulate successful payment (TEST ONLY - bypasses Stripe)
        payment.Status = "succeeded";
        payment.TransactionId = $"test_pi_{Guid.NewGuid().ToString("N")[..24]}";
        payment.UpdatedAt = DateTimeOffset.UtcNow;

        // Update order status to Paid
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

        return Results.Ok(new 
        { 
            message = "Payment confirmed successfully (TEST MODE).", 
            status = "succeeded",
            transactionId = payment.TransactionId
        });
    }
}
