using MediatR;

namespace CampusEats.Api.Features.Users.Create;

public record CreateUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName
) : IRequest<CreateUserResponse>;