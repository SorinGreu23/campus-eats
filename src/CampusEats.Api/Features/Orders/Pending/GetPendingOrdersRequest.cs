using MediatR;
using Microsoft.AspNetCore.Http;

namespace CampusEats.Api.Features.Orders.Pending;

public record GetPendingOrdersRequest : IRequest<IResult>;
