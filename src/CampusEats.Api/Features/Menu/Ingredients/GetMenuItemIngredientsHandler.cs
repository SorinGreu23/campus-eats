using CampusEats.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Menu.Ingredients;

public class GetMenuItemIngredientsHandler : IRequestHandler<GetMenuItemIngredientsRequest, IResult>
{
    private readonly CampusDbContext _db;

    public GetMenuItemIngredientsHandler(CampusDbContext db)
    {
        _db = db;
    }

    public async Task<IResult> Handle(GetMenuItemIngredientsRequest request, CancellationToken cancellationToken)
    {
        var ingredients = await _db.MenuItemIngredients
            .Where(mii => mii.MenuItemId == request.MenuItemId)
            .Select(mii => new
            {
                mii.MenuItemId,
                mii.InventoryItemId,
                InventoryItem = new
                {
                    mii.InventoryItem.Id,
                    mii.InventoryItem.Name,
                    mii.InventoryItem.Unit,
                    mii.InventoryItem.CurrentQuantity,
                    mii.InventoryItem.MinimumQuantity
                },
                mii.QuantityRequired
            })
            .ToListAsync(cancellationToken);

        return Results.Ok(ingredients);
    }
}
