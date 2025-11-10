using CampusEats.Api.Common;
using CampusEats.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Users.Overview;

public class GetUsersHandler : IRequestHandler<GetUsersRequest, Result<GetUsersResponse>>
{
    private readonly IdentityDbContext _identityContext;

    public GetUsersHandler(IdentityDbContext identityContext)
    {
        _identityContext = identityContext;
    }

    public async Task<Result<GetUsersResponse>> Handle(GetUsersRequest request, CancellationToken cancellationToken)
    {
        var users = await _identityContext.Users
            .Select(u => new UserDto(
                u.Id,
                u.Email,
                u.FirstName ?? string.Empty,
                u.LastName ?? string.Empty,
                u.Role,
                u.IsActive,
                u.CreatedAt ?? DateTimeOffset.UtcNow
            ))
            .ToListAsync(cancellationToken);

        return Result<GetUsersResponse>.Success(new GetUsersResponse(users));
    }
}