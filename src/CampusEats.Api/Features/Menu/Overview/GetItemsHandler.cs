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
        var query = _context
            .MenuItems.Include(m => m.Category)
            .Include(m => m.MenuItemAllergens)
                .ThenInclude(mia => mia.Allergen)
            .Include(m => m.MenuItemDietaryRestrictions)
                .ThenInclude(midr => midr.DietaryRestriction)
            .AsQueryable();

        // Filter by category if specified
        if (request.CategoryId.HasValue)
        {
            query = query.Where(m => m.CategoryId == request.CategoryId.Value);
        }

        // Filter by availability if specified
        if (request.IsAvailable.HasValue)
        {
            query = query.Where(m => m.IsAvailable == request.IsAvailable.Value);
        }

        // Filter by dietary restrictions if specified
        // Item must have ALL specified dietary restrictions
        if (request.DietaryRestrictionIds != null && request.DietaryRestrictionIds.Any())
        {
            foreach (var restrictionId in request.DietaryRestrictionIds)
            {
                query = query.Where(m =>
                    m.MenuItemDietaryRestrictions.Any(midr =>
                        midr.DietaryRestrictionId == restrictionId
                    )
                );
            }
        }

        // Filter by search term if specified (name or description)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(m =>
                m.Name.ToLower().Contains(searchLower) ||
                (m.Description != null && m.Description.ToLower().Contains(searchLower))
            );
        }

        var items = await query
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
