using CampusEats.Api.Data;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CampusEats.Api.Common;

namespace CampusEats.Api.Features.Users.Update;

public class UpdateUserHandler : IRequestHandler<UpdateUserRequest, IResult>
{
    private readonly IdentityDbContext _identityContext;
    private readonly IValidator<UpdateUserRequest> _validator;

    public UpdateUserHandler(IdentityDbContext identityContext, IValidator<UpdateUserRequest> validator)
    {
        _identityContext = identityContext;
        _validator = validator;
    }

    public async Task<IResult> Handle(UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
        }

        var user = await _identityContext.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null)
        {
            return Results.BadRequest("User not found");
        }

        if (!string.IsNullOrEmpty(request.FirstName))
            user.FirstName = request.FirstName;

        if (!string.IsNullOrEmpty(request.LastName))
            user.LastName = request.LastName;

        if (request.IsActive.HasValue)
            user.IsActive = request.IsActive.Value;

        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _identityContext.SaveChangesAsync(cancellationToken);

        var response = new UpdateUserResponse(
            user.Id.ToString(),
            user.Email,
            user.FirstName ?? string.Empty,
            user.LastName ?? string.Empty,
            user.IsActive,
            user.UpdatedAt
        );

        return Results.Ok(response);
    }
}
