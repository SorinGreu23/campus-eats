using CampusEats.Api.Common;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Users.Login;

public class LoginHandler : IRequestHandler<LoginRequest, Result<LoginResponse>>
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

    public async Task<Result<LoginResponse>> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<LoginResponse>.Failure(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
        }

        var appUser = await _userManager.FindByEmailAsync(request.Email);

        if (appUser == null)
        {
            return Result<LoginResponse>.Failure("Invalid email or password");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == appUser.UserId, cancellationToken);

        if (user == null || !user.IsActive)
        {
            return Result<LoginResponse>.Failure("Account is inactive or not found");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(appUser, request.Password, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            return Result<LoginResponse>.Failure("Invalid email or password");
        }

        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        var response = new LoginResponse(
            user.Id,
            user.Email,
            user.FirstName ?? string.Empty,
            user.LastName ?? string.Empty,
            user.Role,
            token
        );

        return Result<LoginResponse>.Success(response);
    }
}