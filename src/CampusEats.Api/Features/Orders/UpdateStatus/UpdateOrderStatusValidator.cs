using System;
using System.Linq;
using CampusEats.Api.Features.Orders;
using FluentValidation;

namespace CampusEats.Api.Features.Orders.UpdateStatus;

public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    private static readonly OrderStatus[] AllowedStatuses =
    [
        OrderStatus.Preparing,
        OrderStatus.Ready,
        OrderStatus.Completed,
    ];

    public UpdateOrderStatusValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();

        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(BeValidStatus)
            .WithMessage(
                $"Status must be one of {string.Join(", ", AllowedStatuses.Select(s => s.ToString()))}."
            );
    }

    private static bool BeValidStatus(string? status) =>
        status is not null
        && Enum.TryParse(status, true, out OrderStatus parsed)
        && AllowedStatuses.Contains(parsed);
}
