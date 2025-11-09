using CampusEats.Api.Data;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Menu;

public static class UpdateMenuItem
{
    public record Command(
        Guid Id,
        string Name,
        string? Description,
        decimal Price,
        Guid? CategoryId,
        string? ImageUrl,
        int? PreparationTimeMinutes,
        bool IsAvailable,
        int? Calories
    ) : IRequest<bool>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0");

            RuleFor(x => x.PreparationTimeMinutes)
                .GreaterThanOrEqualTo(0).When(x => x.PreparationTimeMinutes.HasValue)
                .WithMessage("Preparation time must be non-negative");

            RuleFor(x => x.Calories)
                .GreaterThanOrEqualTo(0).When(x => x.Calories.HasValue)
                .WithMessage("Calories must be non-negative");
        }
    }

    public class Handler : IRequestHandler<Command, bool>
    {
        private readonly CampusDbContext _context;
        private readonly IValidator<Command> _validator;

        public Handler(CampusDbContext context, IValidator<Command> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var menuItem = await _context.MenuItems
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (menuItem == null)
                return false;

            menuItem.Name = request.Name;
            menuItem.Description = request.Description;
            menuItem.Price = request.Price;
            menuItem.CategoryId = request.CategoryId;
            menuItem.ImageUrl = request.ImageUrl;
            menuItem.PreparationTimeMinutes = request.PreparationTimeMinutes;
            menuItem.IsAvailable = request.IsAvailable;
            menuItem.Calories = request.Calories;
            menuItem.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}

