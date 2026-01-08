using FluentValidation;

namespace CampusEats.Api.Features.Inventory.Restock;

public class RestockInventoryValidator : AbstractValidator<RestockInventoryRequest>
{
    public RestockInventoryValidator()
    {
        RuleFor(x => x.InventoryItemId)
            .NotEmpty().WithMessage("Inventory item ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters");
    }
}
