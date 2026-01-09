using MediatR;

namespace CampusEats.Api.Features.Payments.TestConfirm;

public class TestConfirmPaymentRequest : IRequest<IResult>
{
    public Guid PaymentId { get; set; }
}
