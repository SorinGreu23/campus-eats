using CampusEats.Api.Common.Interfaces;
using Stripe;

namespace CampusEats.Api.Common.Services;

public class PaymentIntentServiceWrapper : IPaymentIntentService
{
    private readonly PaymentIntentService _paymentIntentService;

    public PaymentIntentServiceWrapper()
    {
        _paymentIntentService = new PaymentIntentService();
    }

    public async Task<PaymentIntent> CreateAsync(PaymentIntentCreateOptions options, CancellationToken cancellationToken = default)
    {
        return await _paymentIntentService.CreateAsync(options, cancellationToken: cancellationToken);
    }

    public async Task<PaymentIntent> GetAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        return await _paymentIntentService.GetAsync(paymentIntentId, cancellationToken: cancellationToken);
    }

    public async Task<PaymentIntent> CancelAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        return await _paymentIntentService.CancelAsync(paymentIntentId, cancellationToken: cancellationToken);
    }
}
