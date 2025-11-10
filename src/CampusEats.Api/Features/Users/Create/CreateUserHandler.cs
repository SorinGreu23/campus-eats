using CampusEats.Api.Common;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CampusEats.Api.Features.Users.Create;

public class CreateUserHandler : IRequestHandler<CreateUserRequest, Result<CreateUserResponse>>
{
    private readonly CampusDbContext _context;
    private readonly IdentityDbContext _identityContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<CreateUserRequest> _validator;

    public CreateUserHandler(
        CampusDbContext context,
        IdentityDbContext identityContext,
        UserManager<ApplicationUser> userManager,
        IValidator<CreateUserRequest> validator)
    {
        _context = context;
        _identityContext = identityContext;
        _userManager = userManager;
        _validator = validator;
    }

    public async Task<Result<CreateUserResponse>> Handle(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<CreateUserResponse>.Failure(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
        }

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser != null)
        {
            return Result<CreateUserResponse>.Failure("User with this email already exists");
        }

        // Start a single transaction on the campus context
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // Step 1: Create the User entity first
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = "Student",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            // Step 2: Make the identity context use the same transaction
            var dbTransaction = transaction.GetDbTransaction();
            await _identityContext.Database.UseTransactionAsync(dbTransaction, cancellationToken);

            // Step 3: Create the ApplicationUser with the User's ID
            var appUser = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                UserId = user.Id
            };

            var result = await _userManager.CreateAsync(appUser, request.Password);

            if (!result.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<CreateUserResponse>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            // Commit the transaction (both contexts)
            await transaction.CommitAsync(cancellationToken);

            var response = new CreateUserResponse(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.CreatedAt!.Value
            );

            return Result<CreateUserResponse>.Success(response);
        }
        catch (Exception ex)
        {
            // Transaction will auto-rollback on dispose if not committed
            return Result<CreateUserResponse>.Failure($"An error occurred while creating the user: {ex.Message}");
        }
    }
}