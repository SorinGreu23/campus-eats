using CampusEats.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Menu.Ingredients;

public class DeleteMenuItemIngredientHandler : IRequestHandler<DeleteMenuItemIngredientRequest, IResult>
{
    private readonly CampusDbContext _db;

    public DeleteMenuItemIngredientHandler(CampusDbContext db)
    {
        _db = db;
    }

    public async Task<IResult> Handle(DeleteMenuItemIngredientRequest request, CancellationToken cancellationToken)
    {
        var ingredient = await _db.MenuItemIngredients
            .FirstOrDefaultAsync(mii => mii.MenuItemId == request.MenuItemId && mii.InventoryItemId == request.InventoryItemId, cancellationToken);

        if (ingredient == null)
            return Results.NotFound(new { error = "Ingredient relationship not found" });

        _db.MenuItemIngredients.Remove(ingredient);
        await _db.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}
