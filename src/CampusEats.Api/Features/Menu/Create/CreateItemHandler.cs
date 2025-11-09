using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using FluentValidation;
using MediatR;

namespace CampusEats.Api.Features.Menu;

public static class CreateItemHandler
{
    public class Handler : IRequestHandler<CreateItemRequest, CreateItemResponse>
    {
        private readonly CampusDbContext _context;
        private readonly IValidator<CreateItemRequest> _validator;

        public Handler(CampusDbContext context, IValidator<CreateItemRequest> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<CreateItemResponse> Handle(CreateItemRequest request, CancellationToken cancellationToken)
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

            return new CreateItemResponse(
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

