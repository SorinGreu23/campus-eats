using MediatR;
using Microsoft.AspNetCore.Http;

namespace CampusEats.Api.Features.Menu;

public record UpdateItemRequest(
    string Name,
    string? Description,
    decimal Price,
    Guid? CategoryId,
    string? ImageUrl,
    int? PreparationTimeMinutes,
    bool IsAvailable,
    int? Calories
);

public record UpdateItemCommand(
    Guid Id,
    UpdateItemRequest Request
) : IRequest<IResult>;

