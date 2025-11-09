using MediatR;

namespace CampusEats.Api.Features.Menu;

public record GetItemsRequest : IRequest<List<GetItemsResponse>>;

