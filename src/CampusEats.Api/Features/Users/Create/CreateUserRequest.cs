using MediatR;
using CampusEats.Api.Common;

namespace CampusEats.Api.Features.Users.Create;

public record CreateUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName
) : IRequest<Result<CreateUserResponse>>;