using MediatR;

namespace CampusEats.Api.Features.Payments.WebhookHandler;

public class StripeWebhookRequest : IRequest<IResult>
{
    public string Payload { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
}
