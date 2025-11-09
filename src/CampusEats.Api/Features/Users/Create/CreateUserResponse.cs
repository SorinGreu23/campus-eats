using MediatR;

namespace CampusEats.Api.Features.Users.Create;

public record CreateUserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    DateTimeOffset CreateAt
);