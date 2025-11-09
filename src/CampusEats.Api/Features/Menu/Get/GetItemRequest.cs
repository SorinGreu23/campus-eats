using MediatR;

namespace CampusEats.Api.Features.Menu;

public record GetItemRequest(Guid Id) : IRequest<GetItemResponse?>;

