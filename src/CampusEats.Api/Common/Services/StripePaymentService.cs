using CampusEats.Api.Common.Interfaces;
using Stripe;

namespace CampusEats.Api.Common.Services;

public class StripePaymentService : IStripePaymentService
{
    private readonly IPaymentIntentService _paymentIntentService;

    public StripePaymentService() : this(new PaymentIntentServiceWrapper())
    {
        var secretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY")
            ?? throw new InvalidOperationException(
                "Stripe secret key is missing. Set the STRIPE_SECRET_KEY environment variable."
            );
        StripeConfiguration.ApiKey = secretKey;
    }

    public StripePaymentService(IPaymentIntentService paymentIntentService)
    {
        _paymentIntentService = paymentIntentService;
    }

    public async Task<string> CreatePaymentIntentAsync(
        decimal amount, 
        string currency, 
        string userId, 
        Guid orderId, 
        CancellationToken cancellationToken = default)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100), // Stripe uses cents
            Currency = currency.ToLower(),
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
            },
            Metadata = new Dictionary<string, string>
            {
                { "userId", userId },
                { "orderId", orderId.ToString() }
            }
        };

        var paymentIntent = await _paymentIntentService.CreateAsync(options, cancellationToken);
        
        return paymentIntent.ClientSecret;
    }

    public async Task<bool> ConfirmPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        var paymentIntent = await _paymentIntentService.GetAsync(paymentIntentId, cancellationToken);
        
        return paymentIntent.Status == "succeeded";
    }

    public async Task<bool> CancelPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var paymentIntent = await _paymentIntentService.CancelAsync(paymentIntentId, cancellationToken);
            return paymentIntent.Status == "canceled";
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetPaymentStatusAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        var paymentIntent = await _paymentIntentService.GetAsync(paymentIntentId, cancellationToken);
        
        return paymentIntent.Status;
    }
}
