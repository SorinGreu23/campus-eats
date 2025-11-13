namespace CampusEats.Api.Features.Users.Get;

public record GetUserResponse(
    string Email,
    string FirstName,
    string LastName,
    string UserName,
    string Role,
    string Token
);