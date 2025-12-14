using CampusEats.Api.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Menu;

public class GetItemsHandler : IRequestHandler<GetItemsRequest, IResult>
{
    private readonly CampusDbContext _context;

    public GetItemsHandler(CampusDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(GetItemsRequest request, CancellationToken cancellationToken)
    {
        var items = await _context
            .MenuItems.Include(m => m.Category)
            .Include(m => m.MenuItemAllergens)
                .ThenInclude(mia => mia.Allergen)
            .Include(m => m.MenuItemDietaryRestrictions)
                .ThenInclude(midr => midr.DietaryRestriction)
            .Select(m => new GetItemsResponse(
                m.Id,
                m.Name,
                m.Description,
                m.Price,
                m.Category != null ? m.Category.Name : null,
                m.ImageUrl,
                m.PreparationTimeMinutes,
                m.IsAvailable,
                m.Calories,
                m.CreatedAt,
                m.UpdatedAt,
                m.MenuItemAllergens.Select(mia => new AllergenDto(
                        mia.Allergen.Id,
                        mia.Allergen.Name,
                        mia.Allergen.Description,
                        mia.Allergen.Icon
                    ))
                    .ToList(),
                m.MenuItemDietaryRestrictions.Select(midr => new DietaryRestrictionDto(
                        midr.DietaryRestriction.Id,
                        midr.DietaryRestriction.Name,
                        midr.DietaryRestriction.Description,
                        midr.DietaryRestriction.Icon
                    ))
                    .ToList()
            ))
            .ToListAsync(cancellationToken);

        return Results.Ok(items);
    }
}
