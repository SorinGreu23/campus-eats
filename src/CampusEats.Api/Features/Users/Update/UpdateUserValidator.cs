using FluentValidation;

namespace CampusEats.Api.Features.Users.Update;

public class UpdateUserValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.FirstName)
            .MaximumLength(128)
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(128)
            .When(x => !string.IsNullOrEmpty(x.LastName));

        RuleFor(x => x.Role)
            .MaximumLength(64)
            .When(x => !string.IsNullOrEmpty(x.Role));
    }
}