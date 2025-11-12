namespace CampusEats.Api.Features.Users.Login;

public record LoginResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string Token
);