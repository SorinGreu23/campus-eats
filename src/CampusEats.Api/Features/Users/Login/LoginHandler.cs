using CampusEats.Api.Common;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Users.Login;

public class LoginHandler : IRequestHandler<LoginRequest, IResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IValidator<LoginRequest> _validator;

    public LoginHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IValidator<LoginRequest> validator)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _validator = validator;
    }

    public async Task<IResult> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
        }

        var appUser = await _userManager.FindByEmailAsync(request.Email);

        if (appUser == null)
        {
            return Results.BadRequest("Invalid email or password");
        }

        if (!appUser.IsActive)
        {
            return Results.BadRequest("Account is inactive");
        }
        
        var roles = await _userManager.GetRolesAsync(appUser);
        var userRole = roles.FirstOrDefault();
        
        if (userRole == null)
        {
            return Results.BadRequest("User has no role assigned.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(appUser, request.Password, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            return Results.BadRequest("Invalid email or password");
        }

        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        var response = new LoginResponse(
            appUser.Id,
            appUser.Email!,
            appUser.FirstName ?? string.Empty,
            appUser.LastName ?? string.Empty,
            userRole,
            token
        );

        return Results.Ok(response);
    }
}