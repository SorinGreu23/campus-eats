using MediatR;

namespace CampusEats.Api.Features.Orders.Get;

public class GetOrdersByUserRequest : IRequest<IResult>
{
    public string UserId { get; set; } = string.Empty;
}
