using FluentValidation;

namespace CampusEats.Api.Features.Users.Create;

public class RegisterValidator : AbstractValidator<RegisterRequest>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .Matches(@"^(?=.{8,100}$)(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).*$")
            .WithMessage(
                "Password must be 8-100 characters and contain at least one lowercase letter, one uppercase letter, one number and one special character"
            );

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .MaximumLength(128);

        RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required").MaximumLength(128);
    }
}
