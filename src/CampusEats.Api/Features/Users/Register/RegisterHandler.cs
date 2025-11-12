using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CampusEats.Api.Common;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CampusEats.Api.Features.Users.Create;

public class RegisterHandler : IRequestHandler<RegisterRequest, IResult>
{
    private readonly CampusDbContext _context;
    private readonly IdentityDbContext _identityContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<RegisterRequest> _validator;

    public RegisterHandler(
        CampusDbContext context,
        IdentityDbContext identityContext,
        UserManager<ApplicationUser> userManager,
        IValidator<RegisterRequest> validator)
    {
        _context = context;
        _identityContext = identityContext;
        _userManager = userManager;
        _validator = validator;
    }

    public async Task<IResult> Handle(RegisterRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Results.BadRequest(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var userId = Guid.NewGuid();

        var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
        var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
        var dbName = Environment.GetEnvironmentVariable("DB_NAME");
        var dbUser = Environment.GetEnvironmentVariable("DB_USER");
        var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

        var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            // Set the connection for both contexts
            _context.Database.SetDbConnection(connection);
            _identityContext.Database.SetDbConnection(connection);

            // Use the shared transaction
            await _context.Database.UseTransactionAsync(transaction, cancellationToken);
            await _identityContext.Database.UseTransactionAsync(transaction, cancellationToken);

            var user = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _identityContext.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var applicationUser = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                UserId = userId
            };

            var identityResult = await _userManager.CreateAsync(applicationUser, request.Password);
            if (!identityResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Results.BadRequest(string.Join(", ", identityResult.Errors.Select(e => e.Description)));
            }

            await transaction.CommitAsync(cancellationToken);

            var response = new RegisterResponse(user.Id, user.Email, user.FirstName, user.LastName, user.CreatedAt);
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            var errorMessage = ex.Message;
            for (var inner = ex.InnerException; inner != null; inner = inner.InnerException)
                errorMessage += $" | Inner: {inner.Message}";

            return Results.InternalServerError($"An error occurred while creating the user: {errorMessage}");
        }
    }
}
