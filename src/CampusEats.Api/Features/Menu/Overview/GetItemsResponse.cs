namespace CampusEats.Api.Features.Menu;

public record GetItemsResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    Guid? CategoryId,
    string? CategoryName,
    string? ImageUrl,
    int? PreparationTimeMinutes,
    bool IsAvailable,
    int? Calories,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt,
    List<AllergenDto>? Allergens,
    List<DietaryRestrictionDto>? DietaryRestrictions
);

