using CampusEats.Api.Common;
using CampusEats.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Users.Delete;

public record DeleteUserResponse(Guid Id, string Message);

public class DeleteUserHandler : IRequestHandler<DeleteUserRequest, Result<DeleteUserResponse>>
{
    private readonly IdentityDbContext _identityContext;

    public DeleteUserHandler(IdentityDbContext identityContext)
    {
        _identityContext = identityContext;
    }

    public async Task<Result<DeleteUserResponse>> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _identityContext.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null)
        {
            return Result<DeleteUserResponse>.Failure("User not found");
        }

        _identityContext.Users.Remove(user);
        await _identityContext.SaveChangesAsync(cancellationToken);

        return Result<DeleteUserResponse>.Success(new DeleteUserResponse(
            user.Id,
            $"User {user.Email} has been deleted successfully"
        ));
    }
}