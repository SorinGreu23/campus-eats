using System.Security.Claims;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Users.Create;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CampusEats.Api.Features.Users.Register;

public class RegisterHandler : IRequestHandler<RegisterRequest, IResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IValidator<RegisterRequest> _validator;

    public RegisterHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IValidator<RegisterRequest> validator
    )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _validator = validator;
    }

    public async Task<IResult> Handle(RegisterRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Results.BadRequest(
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))
            );

        var applicationUser = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true,
        };

        var identityResult = await _userManager.CreateAsync(applicationUser, request.Password);
        if (!identityResult.Succeeded)
        {
            return Results.BadRequest(
                string.Join(", ", identityResult.Errors.Select(e => e.Description))
            );
        }

        // Add claims to the user
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, applicationUser.Id),
            new Claim(ClaimTypes.Email, applicationUser.Email!),
            new Claim(ClaimTypes.GivenName, applicationUser.UserName!),
        };

        var claimsResult = await _userManager.AddClaimsAsync(applicationUser, claims);
        if (!claimsResult.Succeeded)
        {
            await _userManager.DeleteAsync(applicationUser);
            return Results.BadRequest(
                $"Failed to add claims: {string.Join(", ", claimsResult.Errors.Select(e => e.Description))}"
            );
        }

        if (!string.IsNullOrEmpty(request.Role))
        {
            if (!await _roleManager.RoleExistsAsync(request.Role))
            {
                var createRoleResult = await _roleManager.CreateAsync(
                    new IdentityRole(request.Role)
                );
                if (!createRoleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(applicationUser);
                    return Results.BadRequest(
                        $"Failed to create role: {string.Join(", ", createRoleResult.Errors.Select(e => e.Description))}"
                    );
                }
            }

            var roleResult = await _userManager.AddToRoleAsync(applicationUser, request.Role);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(applicationUser);
                return Results.BadRequest(
                    $"Failed to create user: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}"
                );
            }

            // Add role claim
            var roleClaim = new Claim(ClaimTypes.Role, request.Role);
            var roleClaimResult = await _userManager.AddClaimAsync(applicationUser, roleClaim);
            if (!roleClaimResult.Succeeded)
            {
                await _userManager.DeleteAsync(applicationUser);
                return Results.BadRequest(
                    $"Failed to add role claim: {string.Join(", ", roleClaimResult.Errors.Select(e => e.Description))}"
                );
            }
        }

        var response = new RegisterResponse(
            applicationUser.Id,
            applicationUser.Email!,
            applicationUser.FirstName,
            applicationUser.LastName,
            applicationUser.UserName!,
            request.Role
        );

        return Results.Created($"/users/{applicationUser.Id}", response);
    }
}
