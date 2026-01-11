using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Menu;

public class UpdateItemHandler : IRequestHandler<UpdateItemCommand, IResult>
{
    private readonly CampusDbContext _context;
    private readonly IValidator<UpdateItemRequest> _validator;

    public UpdateItemHandler(CampusDbContext context, IValidator<UpdateItemRequest> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<IResult> Handle(
        UpdateItemCommand command,
        CancellationToken cancellationToken
    )
    {
        var validationResult = await _validator.ValidateAsync(command.Request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult
                .Errors.GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return Results.BadRequest(new { errors });
        }

        var menuItem = await _context
            .MenuItems.Include(m => m.MenuItemAllergens)
            .Include(m => m.MenuItemDietaryRestrictions)
            .FirstOrDefaultAsync(m => m.Id == command.Id, cancellationToken);

        if (menuItem == null)
        {
            return Results.NotFound($"Menu item with ID '{command.Id}' was not found.");
        }

        // Keep existing category if none provided to satisfy NOT NULL constraint
        Guid? resolvedCategoryId = command.Request.CategoryId ?? menuItem.CategoryId;
        if (!resolvedCategoryId.HasValue)
        {
            var fallbackCategory = await _context.Categories
                .OrderBy(c => c.DisplayOrder ?? int.MaxValue)
                .FirstOrDefaultAsync(cancellationToken);

            if (fallbackCategory == null)
            {
                return Results.BadRequest(new { error = "Category is required but no categories exist." });
            }

            resolvedCategoryId = fallbackCategory.Id;
        }

        menuItem.Name = command.Request.Name;
        menuItem.Description = command.Request.Description;
        menuItem.Price = command.Request.Price;
        menuItem.CategoryId = resolvedCategoryId;
        menuItem.ImageUrl = command.Request.ImageUrl;
        menuItem.PreparationTimeMinutes = command.Request.PreparationTimeMinutes;
        menuItem.IsAvailable = command.Request.IsAvailable;
        menuItem.Calories = command.Request.Calories;
        menuItem.UpdatedAt = DateTimeOffset.UtcNow;

        // Update allergens
        menuItem.MenuItemAllergens.Clear();
        if (command.Request.AllergenIds?.Count > 0)
        {
            foreach (var allergenId in command.Request.AllergenIds!)
            {
                menuItem.MenuItemAllergens.Add(
                    new MenuItemAllergen { MenuItemId = menuItem.Id, AllergenId = allergenId }
                );
            }
        }

        // Update dietary restrictions
        menuItem.MenuItemDietaryRestrictions.Clear();
        if (command.Request.DietaryRestrictionIds?.Count > 0)
        {
            foreach (var restrictionId in command.Request.DietaryRestrictionIds!)
            {
                menuItem.MenuItemDietaryRestrictions.Add(
                    new MenuItemDietaryRestriction
                    {
                        MenuItemId = menuItem.Id,
                        DietaryRestrictionId = restrictionId,
                    }
                );
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}
