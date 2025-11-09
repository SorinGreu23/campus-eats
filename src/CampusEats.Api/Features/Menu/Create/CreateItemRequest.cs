using MediatR;

public record CreateItemRequest(
        string Name,
        string? Description,
        decimal Price,
        Guid? CategoryId,
        string? ImageUrl,
        int? PreparationTimeMinutes,
        bool IsAvailable,
        int? Calories
    ) : IRequest<CreateItemResponse>;