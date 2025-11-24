using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using FluentValidation;
using MediatR;

namespace CampusEats.Api.Features.InventoryReports.Manage;

public record UpdateInventoryItemQuantityCommand(Guid Id, decimal NewQuantity) : IRequest<IResult>;

public class UpdateInventoryItemQuantityValidator : AbstractValidator<UpdateInventoryItemQuantityCommand>
{
    public UpdateInventoryItemQuantityValidator()
    {
        RuleFor(x => x.NewQuantity).GreaterThanOrEqualTo(0);
    }
}

public class UpdateInventoryItemQuantityHandler : IRequestHandler<UpdateInventoryItemQuantityCommand, IResult>
{
    private readonly CampusDbContext _context;
    private readonly IValidator<UpdateInventoryItemQuantityCommand> _validator;

    public UpdateInventoryItemQuantityHandler(CampusDbContext context, IValidator<UpdateInventoryItemQuantityCommand> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<IResult> Handle(UpdateInventoryItemQuantityCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var item = await _context.InventoryItems.FindAsync(new object[] { request.Id }, cancellationToken);
        if (item == null)
        {
            return Results.NotFound();
        }

        // Log transaction
        var transaction = new InventoryTransaction
        {
            Id = Guid.NewGuid(),
            InventoryItemId = item.Id,
            TransactionType = "ManualAdjustment",
            Quantity = request.NewQuantity - item.CurrentQuantity,
            Reason = "Manual update via API",
        };
        _context.InventoryTransactions.Add(transaction);

        item.CurrentQuantity = request.NewQuantity;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}
