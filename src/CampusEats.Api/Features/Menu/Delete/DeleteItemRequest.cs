using MediatR;
using Microsoft.AspNetCore.Http;

namespace CampusEats.Api.Features.Menu;

public record DeleteItemRequest(Guid Id) : IRequest<IResult>;
