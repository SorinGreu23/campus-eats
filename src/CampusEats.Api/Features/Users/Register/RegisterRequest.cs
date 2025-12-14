using CampusEats.Api.Common;
using MediatR;

namespace CampusEats.Api.Features.Users.Create;

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Role,
    string UserName
) : IRequest<IResult>;
