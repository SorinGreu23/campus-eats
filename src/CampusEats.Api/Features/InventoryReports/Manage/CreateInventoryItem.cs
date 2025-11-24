using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using FluentValidation;
using MediatR;

namespace CampusEats.Api.Features.InventoryReports.Manage;

public record CreateInventoryItemCommand(string Name, string Unit, decimal Quantity, decimal MinimumQuantity) : IRequest<IResult>;

public class CreateInventoryItemValidator : AbstractValidator<CreateInventoryItemCommand>
{
    public CreateInventoryItemValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinimumQuantity).GreaterThanOrEqualTo(0);
    }
}

public class CreateInventoryItemHandler : IRequestHandler<CreateInventoryItemCommand, IResult>
{
    private readonly CampusDbContext _context;
    private readonly IValidator<CreateInventoryItemCommand> _validator;

    public CreateInventoryItemHandler(CampusDbContext context, IValidator<CreateInventoryItemCommand> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<IResult> Handle(CreateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var item = new InventoryItem
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Unit = request.Unit,
            CurrentQuantity = request.Quantity,
            MinimumQuantity = request.MinimumQuantity,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.InventoryItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/inventory/{item.Id}", item);
    }
}
