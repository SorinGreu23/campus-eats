using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Users.Create;

public class CreateUserHandler : IRequestHandler<CreateUserRequest, CreateUserResponse>
{
    private readonly CampusDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<CreateUserRequest> _validator;

    public CreateUserHandler(
        CampusDbContext context,
        UserManager<ApplicationUser> userManager,
        IValidator<CreateUserRequest> validator)
    {
        _context = context;
        _userManager = userManager;
        _validator = validator;
    }

    public async Task<CreateUserResponse> Handle(CreateUserRequest request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = "Customer",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        var appUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(appUser, request.Password);

        if (!result.Succeeded)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        return new CreateUserResponse(
            user.Id,
            user.Email,
            user.FirstName!,
            user.LastName!,
            user.CreatedAt!.Value
        );
    }
}