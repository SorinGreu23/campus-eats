using MediatR;

namespace CampusEats.Api.Features.Payments.ConfirmPayment;

public class ConfirmPaymentRequest : IRequest<IResult>
{
    public Guid PaymentId { get; set; }
    public string PaymentIntentId { get; set; } = string.Empty;
}
