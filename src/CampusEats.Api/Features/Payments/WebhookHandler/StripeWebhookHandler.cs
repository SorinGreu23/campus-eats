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
        var orderId = paymentIntent.Metadata.TryGetValue(Key, out string? value) 
            ? Guid.Parse(value) 
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

            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId.Value, cancellationToken);
            
            if (order != null)
            {
                Console.WriteLine($"[Webhook] Processing order {order.Id} with {order.Items.Count} items");
                
                // Deduct inventory for each order item
                foreach (var orderItem in order.Items)
                {
                    Console.WriteLine($"[Webhook] Processing order item: MenuItemId={orderItem.MenuItemId}, Quantity={orderItem.Quantity}");
                    
                    // Get the ingredients needed for this menu item
                    var ingredients = await _db.MenuItemIngredients
                        .Where(mii => mii.MenuItemId == orderItem.MenuItemId)
                        .ToListAsync(cancellationToken);

                    Console.WriteLine($"[Webhook] Found {ingredients.Count} ingredients for menu item {orderItem.MenuItemId}");

                    foreach (var ingredient in ingredients)
                    {
                        var inventoryItem = await _db.InventoryItems
                            .FirstOrDefaultAsync(i => i.Id == ingredient.InventoryItemId, cancellationToken);

                        if (inventoryItem != null)
                        {
                            var quantityToDeduct = ingredient.QuantityRequired * orderItem.Quantity;
                            Console.WriteLine($"[Webhook] Deducting {quantityToDeduct} {inventoryItem.Unit} of {inventoryItem.Name} (was {inventoryItem.CurrentQuantity})");
                            inventoryItem.CurrentQuantity -= quantityToDeduct;
                            inventoryItem.UpdatedAt = DateTimeOffset.UtcNow;
                            Console.WriteLine($"[Webhook] New quantity: {inventoryItem.CurrentQuantity}");
                        }
                    }
                }

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
