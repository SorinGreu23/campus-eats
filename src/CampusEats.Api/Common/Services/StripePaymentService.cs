using CampusEats.Api.Common.Interfaces;
using Stripe;

namespace CampusEats.Api.Common.Services;

public class StripePaymentService : IStripePaymentService
{
    public StripePaymentService()
    {
        var secretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY")
            ?? throw new InvalidOperationException(
                "Stripe secret key is missing. Set the STRIPE_SECRET_KEY environment variable."
            );
        StripeConfiguration.ApiKey = secretKey;
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

        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(options, cancellationToken: cancellationToken);
        
        return paymentIntent.ClientSecret;
    }

    public async Task<bool> ConfirmPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        var service = new PaymentIntentService();
        var paymentIntent = await service.GetAsync(paymentIntentId, cancellationToken: cancellationToken);
        
        return paymentIntent.Status == "succeeded";
    }

    public async Task<bool> CancelPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        var service = new PaymentIntentService();
        
        try
        {
            var paymentIntent = await service.CancelAsync(paymentIntentId, cancellationToken: cancellationToken);
            return paymentIntent.Status == "canceled";
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetPaymentStatusAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        var service = new PaymentIntentService();
        var paymentIntent = await service.GetAsync(paymentIntentId, cancellationToken: cancellationToken);
        
        return paymentIntent.Status;
    }
}
