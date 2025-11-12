using MediatR;
using CampusEats.Api.Common;

namespace CampusEats.Api.Features.Users.Create;

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string UserName
) : IRequest<IResult>;