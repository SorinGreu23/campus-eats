using Stripe;

namespace CampusEats.Api.Common.Interfaces;

public interface IPaymentIntentService
{
    Task<PaymentIntent> CreateAsync(PaymentIntentCreateOptions options, CancellationToken cancellationToken = default);
    Task<PaymentIntent> GetAsync(string paymentIntentId, CancellationToken cancellationToken = default);
    Task<PaymentIntent> CancelAsync(string paymentIntentId, CancellationToken cancellationToken = default);
}
