using MediatR;

namespace CampusEats.Api.Features.Menu;

public record DeleteItemRequest(Guid Id) : IRequest<bool>;

