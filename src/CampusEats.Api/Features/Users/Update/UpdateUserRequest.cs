using CampusEats.Api.Common;
using MediatR;

namespace CampusEats.Api.Features.Users.Update;

public record UpdateUserRequest(
    string? FirstName,
    string? LastName,
    string? Username,
    string? Role,
    bool? IsActive,
    string? CurrentPassword,
    string? NewPassword
) : IRequest<IResult>;
