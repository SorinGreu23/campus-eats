using MediatR;
using Microsoft.AspNetCore.Http;

namespace CampusEats.Api.Features.Orders.Complete;

public record CompleteOrderRequest(Guid OrderId) : IRequest<IResult>;
