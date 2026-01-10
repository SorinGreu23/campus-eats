using CampusEats.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace CampusEats.Api.Features.Payments.WebhookHandler;

public class StripeWebhookHandler : IRequestHandler<StripeWebhookRequest, IResult>
{
  private const string Key = "orderId";
  private readonly CampusDbContext _db;

    public StripeWebhookHandler(CampusDbContext db)
    {
        _db = db;
    }

    public async Task<IResult> Handle(StripeWebhookRequest request, CancellationToken cancellationToken)
    {
        var webhookSecret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET");
        if (string.IsNullOrEmpty(webhookSecret))
        {
            return Results.Problem("Webhook secret is not configured.");
        }

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                request.Payload,
                request.Signature,
                webhookSecret
            );

            // Handle the event
            if (stripeEvent.Type == "payment_intent.succeeded")
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                if (paymentIntent != null)
                {
                    await HandlePaymentIntentSucceeded(paymentIntent, cancellationToken);
                }
            }
            else if (stripeEvent.Type == "payment_intent.payment_failed")
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                if (paymentIntent != null)
                {
                    await HandlePaymentIntentFailed(paymentIntent, cancellationToken);
                }
            }

            return Results.Ok();
        }
        catch (StripeException e)
        {
            return Results.BadRequest(new { error = e.Message });
        }
    }

    private async Task HandlePaymentIntentSucceeded(PaymentIntent paymentIntent, CancellationToken cancellationToken)
    {
        var orderId = paymentIntent.Metadata.ContainsKey(Key) 
            ? Guid.Parse(paymentIntent.Metadata[Key]) 
            : (Guid?)null;

        if (!orderId.HasValue)
            return;

        var payment = await _db.Payments
            .FirstOrDefaultAsync(p => p.OrderId == orderId.Value && p.Status == "pending", cancellationToken);

        if (payment != null)
        {
            payment.Status = "succeeded";
            payment.TransactionId = paymentIntent.Id;
            payment.UpdatedAt = DateTimeOffset.UtcNow;

            var order = await _db.Orders.FindAsync(new object[] { orderId.Value }, cancellationToken);
            if (order != null)
            {
                order.Status = "Paid";
                order.UpdatedAt = DateTimeOffset.UtcNow;
            }

            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task HandlePaymentIntentFailed(PaymentIntent paymentIntent, CancellationToken cancellationToken)
    {
        var orderId = paymentIntent.Metadata.ContainsKey(Key) 
            ? Guid.Parse(paymentIntent.Metadata[Key]) 
            : (Guid?)null;

        if (!orderId.HasValue)
            return;

        var payment = await _db.Payments
            .FirstOrDefaultAsync(p => p.OrderId == orderId.Value && p.Status == "pending", cancellationToken);

        if (payment != null)
        {
            payment.Status = "failed";
            payment.TransactionId = paymentIntent.Id;
            payment.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
