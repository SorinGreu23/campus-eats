using CampusEats.Api.Data;
using CampusEats.Api.Features.Kitchen;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Orders.UpdateStatus;

public class UpdateOrderStatusHandler : IRequestHandler<UpdateOrderStatusRequest, IResult>
{
    private readonly CampusDbContext _context;
    private readonly IValidator<UpdateOrderStatusRequest> _validator;

    public UpdateOrderStatusHandler(
        CampusDbContext context,
        IValidator<UpdateOrderStatusRequest> validator
    )
    {
        _context = context;
        _validator = validator;
    }

    public async Task<IResult> Handle(
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var order = await _context.Orders.FirstOrDefaultAsync(
            o => o.Id == request.OrderId,
            cancellationToken
        );

        if (order == null)
        {
            return Results.NotFound("Order not found.");
        }

        var currentStatus = Enum.Parse<OrderStatus>(order.Status!);

        if (!Enum.TryParse<OrderStatus>(request.Status, out var newStatus))
        {
            return Results.BadRequest("Invalid order status value.");
        }

        if (!IsValidStatusTransition(currentStatus, newStatus))
        {
            return Results.BadRequest(
                $"Invalid status transition from {currentStatus} to {newStatus}."
            );
        }

        if (newStatus == OrderStatus.Completed)
        {
            order.CompletedAt = DateTimeOffset.UtcNow;
        }

        order.Status = request.Status;
        order.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        return currentStatus switch
        {
            OrderStatus.Pending => newStatus == OrderStatus.Preparing,
            OrderStatus.Preparing => newStatus == OrderStatus.Ready,
            OrderStatus.Ready => newStatus == OrderStatus.Completed,
            _ => false,
        };
    }
}
