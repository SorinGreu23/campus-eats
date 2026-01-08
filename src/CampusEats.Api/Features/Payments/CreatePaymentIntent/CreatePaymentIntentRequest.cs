using MediatR;

namespace CampusEats.Api.Features.Payments.CreatePaymentIntent;

public class CreatePaymentIntentRequest : IRequest<IResult>
{
    public Guid OrderId { get; set; }
}
