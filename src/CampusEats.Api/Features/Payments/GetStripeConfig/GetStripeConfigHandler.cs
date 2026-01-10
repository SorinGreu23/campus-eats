using MediatR;

namespace CampusEats.Api.Features.Payments.GetStripeConfig;

public class GetStripeConfigHandler : IRequestHandler<GetStripeConfigRequest, IResult>
{
    public Task<IResult> Handle(GetStripeConfigRequest request, CancellationToken cancellationToken)
    {
        var publishableKey = Environment.GetEnvironmentVariable("STRIPE_PUBLISHABLE_KEY");

        if (string.IsNullOrEmpty(publishableKey))
        {
            return Task.FromResult(Results.Problem("Stripe configuration is not available."));
        }

        return Task.FromResult(Results.Ok(new { publishableKey }));
    }
}
