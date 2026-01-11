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
        var userIdResult = GetAuthenticatedUserId();
        if (userIdResult.IsFailure)
            return userIdResult.Error;

        var userId = userIdResult.Value;

        var payment = await _db.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

        if (payment is null)
            return Results.NotFound(new { error = "Payment not found." });

        if (payment.UserId != userId)
            return Results.Forbid();

        var isSuccessful = await _stripeService.ConfirmPaymentAsync(request.PaymentIntentId, cancellationToken);

        if (isSuccessful)
        {
            await UpdatePaymentAsSucceededAsync(payment, request.PaymentIntentId, cancellationToken);
            return Results.Ok(new { message = "Payment confirmed successfully.", status = "succeeded" });
        }

        await UpdatePaymentAsFailedAsync(payment, cancellationToken);
        return Results.BadRequest(new { error = "Payment confirmation failed.", status = "failed" });
    }

    private (bool IsFailure, string? Value, IResult? Error) GetAuthenticatedUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null || httpContext.User?.Identity?.IsAuthenticated != true)
            return (true, null, Results.Unauthorized());

        var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return (true, null, Results.Unauthorized());

        return (false, userId, null);
    }

    private async Task UpdatePaymentAsSucceededAsync(Data.Entities.Payment payment, string paymentIntentId, CancellationToken cancellationToken)
    {
        payment.Status = "succeeded";
        payment.TransactionId = paymentIntentId;
        payment.UpdatedAt = DateTimeOffset.UtcNow;

        if (payment.OrderId.HasValue)
        {
            await ProcessOrderPaymentAsync(payment.OrderId.Value, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessOrderPaymentAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order != null && order.Status == "Pending")
        {
            Console.WriteLine($"[ConfirmPayment] Processing order {order.Id} - confirming payment");
            order.Status = "Pending";
            order.UpdatedAt = DateTimeOffset.UtcNow;

            await DeductInventoryForOrderAsync(order, cancellationToken);
        }
    }

    private async Task DeductInventoryForOrderAsync(Data.Entities.Order order, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[ConfirmPayment] Deducting inventory for {order.Items.Count} items");
        
        foreach (var orderItem in order.Items)
        {
            await DeductInventoryForOrderItemAsync(orderItem, cancellationToken);
        }
    }

    private async Task DeductInventoryForOrderItemAsync(Data.Entities.OrderItem orderItem, CancellationToken cancellationToken)
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

    private async Task UpdatePaymentAsFailedAsync(Data.Entities.Payment payment, CancellationToken cancellationToken)
    {
        payment.Status = "failed";
        payment.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
