using MediatR;

namespace CampusEats.Api.Features.Users.Update;

public record UpdateUserRequest(
    Guid Id,
    string? FirstName,
    string? LastName,
    string? Role,
    bool? IsActive
) : IRequest<UpdateUserResponse>;