using CampusEats.Api.Common;
using MediatR;

namespace CampusEats.Api.Features.Users.Update;

public record UpdateUserRequest(
    string Id,
    string? FirstName,
    string? LastName,
    string? Role,
    bool? IsActive
) : IRequest<IResult>;