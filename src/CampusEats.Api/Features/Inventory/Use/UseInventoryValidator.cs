using FluentValidation;

namespace CampusEats.Api.Features.Inventory.Use;

public class UseInventoryValidator : AbstractValidator<UseInventoryRequest>
{
    public UseInventoryValidator()
    {
        RuleFor(x => x.InventoryItemId)
            .NotEmpty()
            .WithMessage("Inventory item ID is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0.");

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .WithMessage("Reason cannot exceed 500 characters.");
    }
}
