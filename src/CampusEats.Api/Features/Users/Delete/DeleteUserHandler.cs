using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CampusEats.Api.Common;
using CampusEats.Api.Data.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CampusEats.Api.Features.Users.Delete;

public class DeleteUserHandler : IRequestHandler<DeleteUserRequest, IResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DeleteUserHandler(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IResult> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
    {
        var currentUser = _httpContextAccessor.HttpContext?.User;
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var currentUserId = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = currentUser.IsInRole("Admin");

        if (currentUserId != request.Id && !isAdmin)
        {
            return Results.Forbid();
        }

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