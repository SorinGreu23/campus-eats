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
            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == payment.OrderId.Value, cancellationToken);
                
            if (order != null && order.Status == "Pending")
            {
                Console.WriteLine($"[TestConfirm] Processing order {order.Id} - confirming test payment");
                order.Status = "Paid";
                order.UpdatedAt = DateTimeOffset.UtcNow;

                // Deduct inventory
                Console.WriteLine($"[TestConfirm] Deducting inventory for {order.Items.Count} items");
                foreach (var orderItem in order.Items)
                {
                    var ingredients = await _db.MenuItemIngredients
                        .Where(mii => mii.MenuItemId == orderItem.MenuItemId)
                        .ToListAsync(cancellationToken);

                    Console.WriteLine($"[TestConfirm] Found {ingredients.Count} ingredients for menu item {orderItem.MenuItemId}");

                    foreach (var ingredient in ingredients)
                    {
                        var inventoryItem = await _db.InventoryItems
                            .FirstOrDefaultAsync(i => i.Id == ingredient.InventoryItemId, cancellationToken);

                        if (inventoryItem != null)
                        {
                            var quantityToDeduct = ingredient.QuantityRequired * orderItem.Quantity;
                            Console.WriteLine($"[TestConfirm] Deducting {quantityToDeduct} {inventoryItem.Unit} of {inventoryItem.Name} (was {inventoryItem.CurrentQuantity})");
                            inventoryItem.CurrentQuantity -= quantityToDeduct;
                            inventoryItem.UpdatedAt = DateTimeOffset.UtcNow;
                            Console.WriteLine($"[TestConfirm] New quantity: {inventoryItem.CurrentQuantity}");
                        }
                    }
                }
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
