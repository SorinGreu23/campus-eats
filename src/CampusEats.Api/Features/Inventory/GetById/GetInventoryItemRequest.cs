using MediatR;
using Microsoft.AspNetCore.Http;

namespace CampusEats.Api.Features.Inventory.GetById;

public record GetInventoryItemRequest(Guid Id) : IRequest<IResult>;
