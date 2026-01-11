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
                var order = await _db.Orders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == payment.OrderId.Value, cancellationToken);
                    
                if (order != null && order.Status == "Pending")
                {
                    Console.WriteLine($"[ConfirmPayment] Processing order {order.Id} - confirming payment");
                    order.Status = "Paid";
                    order.UpdatedAt = DateTimeOffset.UtcNow;

                    // Deduct inventory
                    Console.WriteLine($"[ConfirmPayment] Deducting inventory for {order.Items.Count} items");
                    foreach (var orderItem in order.Items)
                    {
                        var ingredients = await _db.MenuItemIngredients
                            .Where(mii => mii.MenuItemId == orderItem.MenuItemId)
                            .ToListAsync(cancellationToken);

                        Console.WriteLine($"[ConfirmPayment] Found {ingredients.Count} ingredients for menu item {orderItem.MenuItemId}");

                        foreach (var ingredient in ingredients)
                        {
                            var inventoryItem = await _db.InventoryItems
                                .FirstOrDefaultAsync(i => i.Id == ingredient.InventoryItemId, cancellationToken);

                            if (inventoryItem != null)
                            {
                                var quantityToDeduct = ingredient.QuantityRequired * orderItem.Quantity;
                                Console.WriteLine($"[ConfirmPayment] Deducting {quantityToDeduct} {inventoryItem.Unit} of {inventoryItem.Name} (was {inventoryItem.CurrentQuantity})");
                                inventoryItem.CurrentQuantity -= quantityToDeduct;
                                inventoryItem.UpdatedAt = DateTimeOffset.UtcNow;
                                Console.WriteLine($"[ConfirmPayment] New quantity: {inventoryItem.CurrentQuantity}");
                            }
                        }
                    }
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
