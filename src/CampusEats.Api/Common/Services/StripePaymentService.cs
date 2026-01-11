using CampusEats.Api.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace CampusEats.Api.Common.Services;

public class StripePaymentService : IStripePaymentService
{
    private readonly string _secretKey;

    public StripePaymentService(IConfiguration configuration)
    {
        // Try to get from environment variable first (from .env file), then from appsettings
        _secretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY") 
            ?? configuration["Stripe:SecretKey"] 
            ?? throw new InvalidOperationException("Stripe SecretKey is not configured");
        StripeConfiguration.ApiKey = _secretKey;
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
