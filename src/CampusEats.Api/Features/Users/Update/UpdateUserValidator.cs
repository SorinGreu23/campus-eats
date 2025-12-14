using FluentValidation;

namespace CampusEats.Api.Features.Users.Update;

public class UpdateUserValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserValidator()
    {
        // Route `id` is required, not in body. No Id field in request.

        RuleFor(x => x.FirstName)
            .MaximumLength(128)
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName).MaximumLength(128).When(x => !string.IsNullOrEmpty(x.LastName));

        RuleFor(x => x.Role).MaximumLength(64).When(x => !string.IsNullOrEmpty(x.Role));

        RuleFor(x => x.Username)
            .MinimumLength(3)
            .WithMessage("Username must be at least 3 characters")
            .MaximumLength(64)
            .WithMessage("Username must be at most 64 characters")
            .Matches("^[a-zA-Z0-9._-]+$")
            .WithMessage("Username may contain letters, numbers, dots, underscores, and hyphens")
            .When(x => !string.IsNullOrEmpty(x.Username));

        // Password change: require current password when new password is provided
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("Current password is required when changing password")
            .When(x => !string.IsNullOrEmpty(x.NewPassword));

        RuleFor(x => x.NewPassword)
            .MinimumLength(8)
            .WithMessage("New password must be at least 8 characters")
            .Matches("[A-Z]")
            .WithMessage("New password must contain at least one uppercase letter")
            .Matches("[a-z]")
            .WithMessage("New password must contain at least one lowercase letter")
            .Matches("[0-9]")
            .WithMessage("New password must contain at least one number")
            .Matches(@"[!@#$%^&*()_+\-={}:;""'<>,.?/|~`]")
            .WithMessage("New password must contain at least one special character")
            .When(x => !string.IsNullOrEmpty(x.NewPassword));
    }
}
