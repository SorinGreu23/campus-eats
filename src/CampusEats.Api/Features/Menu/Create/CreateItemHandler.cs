using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Menu;

public class CreateItemHandler : IRequestHandler<CreateItemRequest, IResult>
{
    private readonly CampusDbContext _context;
    private readonly IValidator<CreateItemRequest> _validator;

    public CreateItemHandler(CampusDbContext context, IValidator<CreateItemRequest> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<IResult> Handle(CreateItemRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );
            
            return Results.BadRequest(new { errors });
        }

        // Ensure a category is present to satisfy DB NOT NULL constraint
        var categoryId = request.CategoryId;
        if (!categoryId.HasValue)
        {
            var fallbackCategory = await _context.Categories
                .OrderBy(c => c.DisplayOrder ?? int.MaxValue)
                .FirstOrDefaultAsync(cancellationToken);

            if (fallbackCategory == null)
            {
                return Results.BadRequest(new { error = "Category is required but no categories exist." });
            }

            categoryId = fallbackCategory.Id;
        }

        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            CategoryId = categoryId,
            ImageUrl = request.ImageUrl,
            PreparationTimeMinutes = request.PreparationTimeMinutes,
            IsAvailable = request.IsAvailable,
            Calories = request.Calories,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        // Add allergens
        if (request.AllergenIds?.Any() == true)
        {
            foreach (var allergenId in request.AllergenIds)
            {
                menuItem.MenuItemAllergens.Add(new MenuItemAllergen
                {
                    MenuItemId = menuItem.Id,
                    AllergenId = allergenId
                });
            }
        }

        // Add dietary restrictions
        if (request.DietaryRestrictionIds?.Any() == true)
        {
            foreach (var restrictionId in request.DietaryRestrictionIds)
            {
                menuItem.MenuItemDietaryRestrictions.Add(new MenuItemDietaryRestriction
                {
                    MenuItemId = menuItem.Id,
                    DietaryRestrictionId = restrictionId
                });
            }
        }

        _context.MenuItems.Add(menuItem);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new CreateItemResponse(
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
            menuItem.UpdatedAt,
            request.AllergenIds,
            request.DietaryRestrictionIds
        );

        return Results.Created($"/api/menuitems/{response.Id}", response);
    }
}

