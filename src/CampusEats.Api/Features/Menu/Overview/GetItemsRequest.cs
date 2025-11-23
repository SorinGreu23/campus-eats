using MediatR;
using Microsoft.AspNetCore.Http;

namespace CampusEats.Api.Features.Menu;

public record GetItemsRequest : IRequest<IResult>;

