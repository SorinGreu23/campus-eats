using CampusEats.Api.Common.Models;
using CampusEats.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Kitchen;

public class UpdateOrderStatusHandler : IRequestHandler<UpdateOrderStatusCommand, Result>
{
    private readonly CampusDbContext _context;

    public UpdateOrderStatusHandler(CampusDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            return Result.Failure("Order not found.");
        }

        // Validate status transition
        var currentStatus = order.Status ?? "";
        var newStatus = request.Status;

        if (!IsValidStatusTransition(currentStatus, newStatus))
        {
            return Result.Failure($"Invalid status transition from '{currentStatus}' to '{newStatus}'. " +
                "Valid transitions: Pending → Preparing → Ready → Completed");
        }

        order.Status = newStatus;
        order.UpdatedAt = DateTimeOffset.UtcNow;

        // Set CompletedAt when status is Completed
        if (newStatus == "Completed")
        {
            order.CompletedAt = DateTimeOffset.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static bool IsValidStatusTransition(string currentStatus, string newStatus)
    {
        return currentStatus switch
        {
            "Pending" => newStatus == "Preparing",
            "Preparing" => newStatus == "Ready",
            "Ready" => newStatus == "Completed",
            _ => false
        };
    }
}

