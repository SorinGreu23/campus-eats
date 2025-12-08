using MediatR;

namespace CampusEats.Api.Features.Kitchen;

public record GetPendingOrdersQuery : IRequest<IResult>;

