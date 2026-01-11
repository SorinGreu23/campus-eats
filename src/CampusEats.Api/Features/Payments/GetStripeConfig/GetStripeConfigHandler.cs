using MediatR;

namespace CampusEats.Api.Features.Payments.GetStripeConfig;

public class GetStripeConfigHandler : IRequestHandler<GetStripeConfigRequest, IResult>
{
    private readonly IConfiguration _configuration;

    public GetStripeConfigHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<IResult> Handle(GetStripeConfigRequest request, CancellationToken cancellationToken)
    {
        // Try to get from environment variable first (from .env file), then from appsettings
        var publishableKey = Environment.GetEnvironmentVariable("STRIPE_PUBLISHABLE_KEY") 
            ?? _configuration["Stripe:PublishableKey"];

        if (string.IsNullOrEmpty(publishableKey))
        {
            return Task.FromResult(Results.Problem("Stripe configuration is not available."));
        }

        return Task.FromResult(Results.Ok(new { publishableKey }));
    }
}
