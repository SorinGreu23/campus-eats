namespace CampusEats.Api.Features.Users.Get;

public record GetUserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? Role,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);