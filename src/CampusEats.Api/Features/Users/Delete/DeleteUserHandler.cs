using CampusEats.Api.Common;
using CampusEats.Api.Data.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CampusEats.Api.Features.Users.Delete;

public class DeleteUserHandler : IRequestHandler<DeleteUserRequest, IResult>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DeleteUserHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IResult> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id);

        if (user == null)
        {
            return Results.NotFound($"User with id {request.Id} not found");
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return Results.BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return Results.NoContent();
    }
}