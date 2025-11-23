using CampusEats.Api.Data;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CampusEats.Api.Common;

namespace CampusEats.Api.Features.Users.Update;

public class UpdateUserHandler : IRequestHandler<UpdateUserRequest, Result<UpdateUserResponse>>
{
    private readonly IdentityDbContext _identityContext;
    private readonly IValidator<UpdateUserRequest> _validator;

    public UpdateUserHandler(IdentityDbContext identityContext, IValidator<UpdateUserRequest> validator)
    {
        _identityContext = identityContext;
        _validator = validator;
    }

    public async Task<Result<UpdateUserResponse>> Handle(UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<UpdateUserResponse>.Failure(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
        }

        var user = await _identityContext.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null)
        {
            return Result<UpdateUserResponse>.Failure("User not found");
        }

        if (!string.IsNullOrEmpty(request.FirstName))
            user.FirstName = request.FirstName;

        if (!string.IsNullOrEmpty(request.LastName))
            user.LastName = request.LastName;

        if (!string.IsNullOrEmpty(request.Role))
            user.Role = request.Role;

        if (request.IsActive.HasValue)
            user.IsActive = request.IsActive.Value;

        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _identityContext.SaveChangesAsync(cancellationToken);

        var response = new UpdateUserResponse(
            user.Id,
            user.Email,
            user.FirstName ?? string.Empty,
            user.LastName ?? string.Empty,
            user.Role ?? string.Empty,
            user.IsActive,
            user.UpdatedAt.Value
        );

        return Result<UpdateUserResponse>.Success(response);
    }
}