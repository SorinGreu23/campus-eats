using CampusEats.Api.Data;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Kitchen;

public class UpdateOrderStatusHandler : IRequestHandler<UpdateOrderStatusCommand, IResult>
{
    private readonly CampusDbContext _context;
    private readonly IValidator<UpdateOrderStatusCommand> _validator;

    public UpdateOrderStatusHandler(CampusDbContext context, IValidator<UpdateOrderStatusCommand> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<IResult> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Results.BadRequest(new { errors });
        }

        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            return Results.NotFound(new { message = "Order not found." });
        }

        var currentStatus = Enum.TryParse<OrderStatus>(order.Status, out var parsedCurrent)
            ? parsedCurrent
            : OrderStatus.Pending;

        var newStatus = request.Status;

        if (!IsValidStatusTransition(currentStatus, newStatus))
        {
            return Results.BadRequest(new
            {
                message = $"Invalid status transition from '{currentStatus}' to '{newStatus}'.",
                validTransitions = "Pending → Preparing → Ready → Completed"
            });
        }

        order.Status = newStatus.ToString();
        order.UpdatedAt = DateTimeOffset.UtcNow;

        if (newStatus == OrderStatus.Completed)
        {
            order.CompletedAt = DateTimeOffset.UtcNow;
        }

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
            _ => false
        };
    }
}

