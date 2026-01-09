using CampusEats.Api.Common.Interfaces;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Payments.CreatePaymentIntent;

public class CreatePaymentIntentHandler : IRequestHandler<CreatePaymentIntentRequest, IResult>
{
    private readonly CampusDbContext _db;
    private readonly IStripePaymentService _stripeService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreatePaymentIntentHandler(
        CampusDbContext db, 
        IStripePaymentService stripeService,
        IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _stripeService = stripeService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IResult> Handle(CreatePaymentIntentRequest request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null || httpContext.User?.Identity?.IsAuthenticated != true)
            return Results.Unauthorized();

        var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        // Find the order
        var order = await _db.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
            return Results.NotFound(new { error = "Order not found." });

        // Verify the user owns this order
        if (order.UserId != userId)
            return Results.Forbid();

        // Check if order is in a valid state for payment
        if (order.Status != "Pending")
            return Results.BadRequest(new { error = "Order is not in a valid state for payment." });

        // Create payment intent in Stripe
        var clientSecret = await _stripeService.CreatePaymentIntentAsync(
            order.Total,
            "usd",
            userId,
            order.Id,
            cancellationToken
        );

        // Create payment record in database
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            UserId = userId,
            Amount = order.Total,
            Status = "pending",
            PaymentMethod = "stripe",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync(cancellationToken);

        return Results.Ok(new 
        { 
            clientSecret,
            paymentId = payment.Id,
            amount = order.Total
        });
    }
}
