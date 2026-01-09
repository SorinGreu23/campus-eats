namespace CampusEats.Api.Common.Interfaces;

public interface IStripePaymentService
{
    Task<string> CreatePaymentIntentAsync(decimal amount, string currency, string userId, Guid orderId, CancellationToken cancellationToken = default);
    Task<bool> ConfirmPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default);
    Task<bool> CancelPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default);
    Task<string> GetPaymentStatusAsync(string paymentIntentId, CancellationToken cancellationToken = default);
}
