namespace CampusEats.Api.Features.Menu;

public record GetItemResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
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

