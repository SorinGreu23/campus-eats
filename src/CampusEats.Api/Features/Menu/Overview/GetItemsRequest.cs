using MediatR;

namespace CampusEats.Api.Features.Menu;

public record GetItemsRequest(
    Guid? CategoryId,
    List<Guid>? AllergenIds,
    List<Guid>? DietaryRestrictionIds,
    bool? IsAvailable
) : IRequest<List<GetItemsResponse>>;

