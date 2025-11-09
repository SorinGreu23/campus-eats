using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using FluentValidation;
using MediatR;

namespace CampusEats.Api.Features.Menu;

public static class CreateMenuItem
{
    public record Command(
        string Name,
        string? Description,
        decimal Price,
        Guid? CategoryId,
        string? ImageUrl,
        int? PreparationTimeMinutes,
        bool IsAvailable,
        int? Calories
    ) : IRequest<Response>;

    public record Response(
        Guid Id,
        string Name,
        string? Description,
        decimal Price,
        Guid? CategoryId,
        string? ImageUrl,
        int? PreparationTimeMinutes,
        bool IsAvailable,
        int? Calories,
        DateTimeOffset? CreatedAt,
        DateTimeOffset? UpdatedAt
    );

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
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

    public class Handler : IRequestHandler<Command, Response>
    {
        private readonly CampusDbContext _context;
        private readonly IValidator<Command> _validator;

        public Handler(CampusDbContext context, IValidator<Command> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var menuItem = new MenuItem
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                CategoryId = request.CategoryId,
                ImageUrl = request.ImageUrl,
                PreparationTimeMinutes = request.PreparationTimeMinutes,
                IsAvailable = request.IsAvailable,
                Calories = request.Calories,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync(cancellationToken);

            return new Response(
                menuItem.Id,
                menuItem.Name,
                menuItem.Description,
                menuItem.Price,
                menuItem.CategoryId,
                menuItem.ImageUrl,
                menuItem.PreparationTimeMinutes,
                menuItem.IsAvailable,
                menuItem.Calories,
                menuItem.CreatedAt,
                menuItem.UpdatedAt
            );
        }
    }
}

