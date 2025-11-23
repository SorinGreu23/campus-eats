using CampusEats.Api.Common;
using CampusEats.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Users.Get;

public class GetUserHandler : IRequestHandler<GetUserRequest, Result<GetUserResponse>>
{
    private readonly IdentityDbContext _identityContext;

    public GetUserHandler(IdentityDbContext identityContext)
    {
        _identityContext = identityContext;
    }

    public async Task<Result<GetUserResponse>> Handle(GetUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _identityContext.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null)
        {
            return Result<GetUserResponse>.Failure("User not found");
        }

        var response = new GetUserResponse(
            user.Id,
            user.Email,
            user.FirstName ?? string.Empty,
            user.LastName ?? string.Empty,
            user.Role,
            user.IsActive,
            user.CreatedAt ?? DateTimeOffset.UtcNow,
            user.UpdatedAt
        );

        return Result<GetUserResponse>.Success(response);
    }
}