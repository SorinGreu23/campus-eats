using MediatR;

namespace CampusEats.Api.Features.Orders.UpdateStatus;

public record UpdateOrderStatusRequest(Guid OrderId, string Status) : IRequest<IResult>;

