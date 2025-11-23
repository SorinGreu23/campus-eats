namespace CampusEats.Api.Features.Menu;

public record GetItemsResponse(
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
    DateTimeOffset? UpdatedAt
);

