using MediatR;
using Microsoft.AspNetCore.Http;

namespace CampusEats.Api.Features.Inventory.Get;

public record GetInventoryItemsRequest : IRequest<IResult>;
