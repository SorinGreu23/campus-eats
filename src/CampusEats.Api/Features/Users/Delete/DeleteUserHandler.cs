using CampusEats.Api.Common;
using CampusEats.Api.Data;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Users.Delete;

public class DeleteUserHandler : IRequestHandler<DeleteUserRequest, IResult>
{
    private readonly IdentityDbContext _identityContext;

    public DeleteUserHandler(IdentityDbContext identityContext)
    {
        _identityContext = identityContext;
    }

    public async Task<IResult> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _identityContext.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null)
        {
            return Results.NotFound($"User with id {request.Id} not found");
        }

        _identityContext.Users.Remove(user);
        await _identityContext.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}