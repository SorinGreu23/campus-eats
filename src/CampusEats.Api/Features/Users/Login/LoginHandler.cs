using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Users.Login;

public class LoginHandler : IRequestHandler<LoginRequest, LoginResponse>
{
    private readonly CampusDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IValidator<LoginRequest> _validator;

    public LoginHandler(
        CampusDbContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IValidator<LoginRequest> validator)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
        _validator = validator;
    }

    public async Task<LoginResponse> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var appUser = await _userManager.FindByEmailAsync(request.Email);

        if (appUser == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == appUser.UserId, cancellationToken);

        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Account is inactive or not found");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(appUser, request.Password, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        return new LoginResponse(
            user.Id,
            user.Email,
            user.FirstName ?? string.Empty,
            user.LastName ?? string.Empty,
            user.Role,
            token
        );
    }
}