namespace CampusEats.Api.Features.Users.Update;

public record UpdateUserResponse(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive
);