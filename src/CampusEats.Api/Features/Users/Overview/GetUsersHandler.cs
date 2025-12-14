using CampusEats.Api.Common;
using CampusEats.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Users.Overview;

public class GetUsersHandler : IRequestHandler<GetUsersQuery, IResult>
{
    private readonly IdentityDbContext _identityContext;

    public GetUsersHandler(IdentityDbContext identityContext)
    {
        _identityContext = identityContext;
    }

    public async Task<IResult> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        // Query ApplicationUser (AspNetUsers table) with their roles from AspNetUserRoles
        var users = await _identityContext
            .Users.Select(u => new UserDto(
                u.Id,
                u.Email ?? string.Empty,
                u.FirstName ?? string.Empty,
                u.LastName ?? string.Empty,
                _identityContext
                    .UserRoles.Where(ur => ur.UserId == u.Id)
                    .Join(_identityContext.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                    .FirstOrDefault()
                    ?? string.Empty,
                u.IsActive
            ))
            .ToListAsync(cancellationToken);

        return Results.Ok(new GetUsersResponse(users));
    }
}
