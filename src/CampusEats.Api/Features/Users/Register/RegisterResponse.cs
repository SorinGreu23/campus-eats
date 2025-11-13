using MediatR;

namespace CampusEats.Api.Features.Users.Create;

public record RegisterResponse(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string UserName,
    string Role,
    DateTimeOffset CreateAt
);