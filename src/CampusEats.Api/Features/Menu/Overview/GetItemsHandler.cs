using CampusEats.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Menu;

public class GetItemsHandler : IRequestHandler<GetItemsRequest, List<GetItemsResponse>>
{
    private readonly CampusDbContext _context;

    public GetItemsHandler(CampusDbContext context)
    {
        _context = context;
    }

    public async Task<List<GetItemsResponse>> Handle(GetItemsRequest request, CancellationToken cancellationToken)
    {
        var query = _context.MenuItems
            .Include(m => m.Category)
            .Include(m => m.MenuItemAllergens)
                .ThenInclude(ma => ma.Allergen)
            .Include(m => m.MenuItemDietaryRestrictions)
                .ThenInclude(md => md.DietaryRestriction)
            .AsQueryable();

        // Filter by category if provided
        if (request.CategoryId.HasValue)
        {
            query = query.Where(m => m.CategoryId == request.CategoryId.Value);
        }

        // Filter by availability if provided
        if (request.IsAvailable.HasValue)
        {
            query = query.Where(m => m.IsAvailable == request.IsAvailable.Value);
        }

        // Filter by allergens - exclude items that contain these allergens
        if (request.AllergenIds != null && request.AllergenIds.Any())
        {
            query = query.Where(m => !m.MenuItemAllergens.Any(ma => request.AllergenIds.Contains(ma.AllergenId)));
        }

        // Filter by dietary restrictions - include items that match ALL specified dietary restrictions
        if (request.DietaryRestrictionIds != null && request.DietaryRestrictionIds.Any())
        {
            foreach (var dietaryRestrictionId in request.DietaryRestrictionIds)
            {
                query = query.Where(m => m.MenuItemDietaryRestrictions.Any(md => md.DietaryRestrictionId == dietaryRestrictionId));
            }
        }

        return await query
            .Select(m => new GetItemsResponse(
                m.Id,
                m.Name,
                m.Description,
                m.Price,
                m.CategoryId,
                m.Category != null ? m.Category.Name : null,
                m.ImageUrl,
                m.PreparationTimeMinutes,
                m.IsAvailable,
                m.Calories,
                m.CreatedAt,
                m.UpdatedAt,
                m.MenuItemAllergens.Select(ma => new AllergenDto(
                    ma.Allergen.Id,
                    ma.Allergen.Name,
                    ma.Allergen.Description
                )).ToList(),
                m.MenuItemDietaryRestrictions.Select(md => new DietaryRestrictionDto(
                    md.DietaryRestriction.Id,
                    md.DietaryRestriction.Name,
                    md.DietaryRestriction.Description
                )).ToList()
            ))
            .ToListAsync(cancellationToken);
    }
}

