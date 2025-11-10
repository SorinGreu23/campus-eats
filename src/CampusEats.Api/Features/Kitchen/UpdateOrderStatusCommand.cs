using CampusEats.Api.Common.Models;
using MediatR;

namespace CampusEats.Api.Features.Kitchen;

public record UpdateOrderStatusCommand(Guid OrderId, OrderStatus Status) : IRequest<Result>;

