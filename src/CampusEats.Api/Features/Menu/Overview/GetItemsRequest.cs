using MediatR;
using Microsoft.AspNetCore.Http;

namespace CampusEats.Api.Features.Menu;

/// <summary>
/// Request to get menu items with optional filtering by category and dietary restrictions
/// </summary>
/// <param name="CategoryId">Optional category ID to filter menu items</param>
/// <param name="DietaryRestrictionIds">Optional array of dietary restriction IDs - items must match ALL specified restrictions</param>
/// <param name="IsAvailable">Optional filter for item availability status</param>
public record GetItemsRequest(
    Guid? CategoryId = null,
    Guid[]? DietaryRestrictionIds = null,
    bool? IsAvailable = null
) : IRequest<IResult>;
