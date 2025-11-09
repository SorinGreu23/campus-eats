using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Menu;

public class CreateItemHandler : IRequestHandler<CreateItemRequest, CreateItemResponse>
{
    private readonly CampusDbContext _context;
    private readonly IValidator<CreateItemRequest> _validator;

    public CreateItemHandler(CampusDbContext context, IValidator<CreateItemRequest> validator)
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
        
        // Add allergens if provided
        if (request.AllergenIds != null && request.AllergenIds.Any())
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
        
        // Add dietary restrictions if provided
        if (request.DietaryRestrictionIds != null && request.DietaryRestrictionIds.Any())
        {
            foreach (var dietaryRestrictionId in request.DietaryRestrictionIds)
            {
                menuItem.MenuItemDietaryRestrictions.Add(new MenuItemDietaryRestriction
                {
                    MenuItemId = menuItem.Id,
                    DietaryRestrictionId = dietaryRestrictionId
                });
            }
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        
        // Reload with related data
        var savedMenuItem = await _context.MenuItems
            .Include(m => m.MenuItemAllergens)
                .ThenInclude(ma => ma.Allergen)
            .Include(m => m.MenuItemDietaryRestrictions)
                .ThenInclude(md => md.DietaryRestriction)
            .FirstAsync(m => m.Id == menuItem.Id, cancellationToken);

        return new CreateItemResponse(
            savedMenuItem.Id,
            savedMenuItem.Name,
            savedMenuItem.Description,
            savedMenuItem.Price,
            savedMenuItem.CategoryId,
            savedMenuItem.ImageUrl,
            savedMenuItem.PreparationTimeMinutes,
            savedMenuItem.IsAvailable,
            savedMenuItem.Calories,
            savedMenuItem.CreatedAt,
            savedMenuItem.UpdatedAt,
            savedMenuItem.MenuItemAllergens.Select(ma => new AllergenDto(
                ma.Allergen.Id,
                ma.Allergen.Name,
                ma.Allergen.Description
            )).ToList(),
            savedMenuItem.MenuItemDietaryRestrictions.Select(md => new DietaryRestrictionDto(
                md.DietaryRestriction.Id,
                md.DietaryRestriction.Name,
                md.DietaryRestriction.Description
            )).ToList()
        );
    }
}

