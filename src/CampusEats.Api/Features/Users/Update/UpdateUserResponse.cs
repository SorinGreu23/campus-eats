namespace CampusEats.Api.Features.Users.Update;

public record UpdateUserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? Role,
    bool IsActive,
    DateTimeOffset UpdatedAt
);