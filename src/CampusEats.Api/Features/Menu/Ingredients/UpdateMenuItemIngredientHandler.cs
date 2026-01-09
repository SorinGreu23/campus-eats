using CampusEats.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Menu.Ingredients;

public class UpdateMenuItemIngredientHandler : IRequestHandler<UpdateMenuItemIngredientRequest, IResult>
{
    private readonly CampusDbContext _db;

    public UpdateMenuItemIngredientHandler(CampusDbContext db)
    {
        _db = db;
    }

    public async Task<IResult> Handle(UpdateMenuItemIngredientRequest request, CancellationToken cancellationToken)
    {
        var ingredient = await _db.MenuItemIngredients
            .FirstOrDefaultAsync(mii => mii.MenuItemId == request.MenuItemId && mii.InventoryItemId == request.InventoryItemId, cancellationToken);

        if (ingredient == null)
            return Results.NotFound(new { error = "Ingredient relationship not found" });

        ingredient.QuantityRequired = request.QuantityRequired;
        await _db.SaveChangesAsync(cancellationToken);

        return Results.Ok(new
        {
            ingredient.MenuItemId,
            ingredient.InventoryItemId,
            ingredient.QuantityRequired
        });
    }
}
