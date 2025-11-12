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
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;

    public GetUserHandler(UserManager<User> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<IResult> Handle(GetUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        
        if (user == null)
        {
            return Results.NotFound("User not found.");
        }
        
        return Results.Ok(new GetUserResponse
        (
            user.Email,
            user.FirstName,
            user.LastName,
            user.UserName,
            _tokenService.CreateToken(user)
        ));
    }
}