using CampusEats.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Menu;

public class GetItemHandler : IRequestHandler<GetItemRequest, GetItemResponse?>
{
    private readonly CampusDbContext _context;

    public GetItemHandler(CampusDbContext context)
    {
        _context = context;
    }

    public async Task<GetItemResponse?> Handle(GetItemRequest request, CancellationToken cancellationToken)
    {
        var menuItem = await _context.MenuItems
            .Include(m => m.Category)
            .Include(m => m.MenuItemAllergens)
                .ThenInclude(ma => ma.Allergen)
            .Include(m => m.MenuItemDietaryRestrictions)
                .ThenInclude(md => md.DietaryRestriction)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (menuItem == null)
            return null;

        return new GetItemResponse(
            menuItem.Id,
            menuItem.Name,
            menuItem.Description,
            menuItem.Price,
            menuItem.CategoryId,
            menuItem.Category?.Name,
            menuItem.ImageUrl,
            menuItem.PreparationTimeMinutes,
            menuItem.IsAvailable,
            menuItem.Calories,
            menuItem.CreatedAt,
            menuItem.UpdatedAt,
            menuItem.MenuItemAllergens.Select(ma => new AllergenDto(
                ma.Allergen.Id,
                ma.Allergen.Name,
                ma.Allergen.Description
            )).ToList(),
            menuItem.MenuItemDietaryRestrictions.Select(md => new DietaryRestrictionDto(
                md.DietaryRestriction.Id,
                md.DietaryRestriction.Name,
                md.DietaryRestriction.Description
            )).ToList()
        );
    }
}

