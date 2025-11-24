using CampusEats.Api.Common;
using CampusEats.Api.Common.Interfaces;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Users.Get;

public class GetUserHandler : IRequestHandler<GetUserRequest, IResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public GetUserHandler(UserManager<ApplicationUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<IResult> Handle(GetUserRequest request, CancellationToken cancellationToken)
    {
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
        
        return Results.Ok(new GetUserResponse
        (
            appUser.Email,
            appUser.FirstName ?? string.Empty,
            appUser.LastName ?? string.Empty,
            appUser.UserName!,
            userRole,
            _tokenService.CreateToken(appUser)
        ));
    }
}