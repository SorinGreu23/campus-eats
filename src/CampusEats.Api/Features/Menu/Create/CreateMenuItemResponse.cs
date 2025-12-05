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
        List<Guid>? AllergenIds,
        List<Guid>? DietaryRestrictionIds
    );