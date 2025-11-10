using FluentValidation;

namespace CampusEats.Api.Features.Kitchen;

public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    private static readonly OrderStatus[] ValidTransitions = { OrderStatus.Preparing, OrderStatus.Ready, OrderStatus.Completed };

    public UpdateOrderStatusValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required.");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid order status.")
            .Must(status => ValidTransitions.Contains(status))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidTransitions)}");
    }
}

