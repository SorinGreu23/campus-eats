using FluentValidation;

namespace CampusEats.Api.Features.Menu;

public class CreateItemValidator : AbstractValidator<CreateItemRequest>
{
    public CreateItemValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(100)
            .WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.PreparationTimeMinutes)
            .GreaterThanOrEqualTo(0)
            .When(x => x.PreparationTimeMinutes.HasValue)
            .WithMessage("Preparation time must be non-negative");

        RuleFor(x => x.Calories)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Calories.HasValue)
            .WithMessage("Calories must be non-negative");
    }
}
