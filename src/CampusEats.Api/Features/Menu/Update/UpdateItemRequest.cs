using MediatR;

namespace CampusEats.Api.Features.Menu;

public record UpdateItemRequest(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    Guid? CategoryId,
    string? ImageUrl,
    int? PreparationTimeMinutes,
    bool IsAvailable,
    int? Calories,
    List<Guid>? AllergenIds,
    List<Guid>? DietaryRestrictionIds
) : IRequest<bool>;

