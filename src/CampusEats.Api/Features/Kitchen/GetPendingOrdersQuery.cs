using CampusEats.Api.Common.Models;
using MediatR;

namespace CampusEats.Api.Features.Kitchen;

public record GetPendingOrdersQuery : IRequest<Result<List<PendingOrderDto>>>;

