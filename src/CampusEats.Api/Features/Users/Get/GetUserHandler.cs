using CampusEats.Api.Common;
using CampusEats.Api.Common.Interfaces;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Users.Get;

public class GetUserHandler : IRequestHandler<GetUserRequest, IResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetUserHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IHttpContextAccessor httpContextAccessor
    )
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IResult> Handle(GetUserRequest request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null || httpContext.User?.Identity?.IsAuthenticated != true)
        {
            return Results.Unauthorized();
        }

        var principal = httpContext.User;
        var currentUser = await _userManager.GetUserAsync(principal);
        var isAdmin = currentUser != null && await _userManager.IsInRoleAsync(currentUser, "Admin");
        var currentUserId = _userManager.GetUserId(principal);
        if (!isAdmin && currentUserId != request.Id)
        {
            return Results.Forbid();
        }
        else
        {
            throw new InvalidOperationException(
                "Unable to determine user roles; cannot proceed with user retrieval."
            );
        }

        var appUser = await _userManager.FindByIdAsync(request.Id);

        if (appUser == null)
        {
            return Results.NotFound("User not found.");
        }

        if (string.IsNullOrEmpty(appUser.Email))
        {
            return Results.Problem("User email is not configured.", statusCode: 500);
        }

        var roles = await _userManager.GetRolesAsync(appUser);
        var userRole = roles.FirstOrDefault();

        if (string.IsNullOrEmpty(userRole))
        {
            return Results.Problem("User role is not assigned.", statusCode: 500);
        }

        return Results.Ok(
            new GetUserResponse(
                appUser.Email,
                appUser.FirstName ?? string.Empty,
                appUser.LastName ?? string.Empty,
                appUser.UserName!,
                userRole
            )
        );
    }
}
