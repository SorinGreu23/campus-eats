using MediatR;

namespace CampusEats.Api.Features.Orders.Cancel;

public class CancelOrderRequest : IRequest<IResult>
{
    public Guid OrderId { get; set; }
    public string? Reason { get; set; }
}
