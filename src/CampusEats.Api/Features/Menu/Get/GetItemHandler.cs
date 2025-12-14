using CampusEats.Api.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Menu;

public class GetItemHandler : IRequestHandler<GetItemRequest, IResult>
{
    private readonly CampusDbContext _context;

    public GetItemHandler(CampusDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(GetItemRequest request, CancellationToken cancellationToken)
    {
        var menuItem = await _context
            .MenuItems.Include(m => m.Category)
            .Include(m => m.MenuItemAllergens)
                .ThenInclude(mia => mia.Allergen)
            .Include(m => m.MenuItemDietaryRestrictions)
                .ThenInclude(midr => midr.DietaryRestriction)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (menuItem == null)
        {
            return Results.NotFound($"Menu item with ID '{request.Id}' was not found.");
        }

        var response = new GetItemResponse(
            menuItem.Id,
            menuItem.Name,
            menuItem.Description,
            menuItem.Price,
            menuItem.Category?.Name,
            menuItem.ImageUrl,
            menuItem.PreparationTimeMinutes,
            menuItem.IsAvailable,
            menuItem.Calories,
            menuItem.CreatedAt,
            menuItem.UpdatedAt,
            menuItem
                .MenuItemAllergens?.Select(mia => new AllergenDto(
                    mia.Allergen.Id,
                    mia.Allergen.Name,
                    mia.Allergen.Description,
                    mia.Allergen.Icon
                ))
                .ToList(),
            menuItem
                .MenuItemDietaryRestrictions?.Select(midr => new DietaryRestrictionDto(
                    midr.DietaryRestriction.Id,
                    midr.DietaryRestriction.Name,
                    midr.DietaryRestriction.Description,
                    midr.DietaryRestriction.Icon
                ))
                .ToList()
        );

        return Results.Ok(response);
    }
}
