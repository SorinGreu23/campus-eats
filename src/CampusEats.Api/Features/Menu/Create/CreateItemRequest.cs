using MediatR;
using Microsoft.AspNetCore.Http;

namespace CampusEats.Api.Features.Menu;

public record CreateItemRequest(
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
    ) : IRequest<IResult>;
