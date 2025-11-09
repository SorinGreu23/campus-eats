namespace CampusEats.Api.Features.Menu;

public record CreateItemResponse(
        Guid Id,
        string Name,
        string? Description,
        decimal Price,
        Guid? CategoryId,
        string? ImageUrl,
        int? PreparationTimeMinutes,
        bool IsAvailable,
        int? Calories,
        DateTimeOffset? CreatedAt,
        DateTimeOffset? UpdatedAt,
        List<AllergenDto>? Allergens,
        List<DietaryRestrictionDto>? DietaryRestrictions
    );

public record AllergenDto(Guid Id, string Name, string? Description);
public record DietaryRestrictionDto(Guid Id, string Name, string? Description);
