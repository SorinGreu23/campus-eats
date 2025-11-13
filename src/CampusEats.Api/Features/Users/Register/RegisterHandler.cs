using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CampusEats.Api.Common;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CampusEats.Api.Features.Users.Create;

public class RegisterHandler : IRequestHandler<RegisterRequest, IResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<RegisterRequest> _validator;
    private readonly IdentityDbContext _identityDbContext;

    public RegisterHandler(
        UserManager<ApplicationUser> userManager,
        IValidator<RegisterRequest> validator,
        IdentityDbContext identityDbContext)
    {
        _userManager = userManager;
        _validator = validator;
        _identityDbContext = identityDbContext;
    }

    public async Task<IResult> Handle(RegisterRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Results.BadRequest(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var applicationUser = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
        };

        var identityResult = await _userManager.CreateAsync(applicationUser, request.Password);
        if (!identityResult.Succeeded)
        {
            return Results.BadRequest(string.Join(", ", identityResult.Errors.Select(e => e.Description)));
        }

        var user = new User
        {
            Id = applicationUser.UserId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true,
            Role = request.Role,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var response = new RegisterResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            applicationUser.UserName,
            user.Role,
            user.CreatedAt
            );
            
        return Results.Ok(response);
    }
}