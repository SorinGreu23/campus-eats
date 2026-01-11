using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Menu.Ingredients;

public class AddMenuItemIngredientHandler : IRequestHandler<AddMenuItemIngredientRequest, IResult>
{
    private readonly CampusDbContext _db;

    public AddMenuItemIngredientHandler(CampusDbContext db)
    {
        _db = db;
    }

    public async Task<IResult> Handle(AddMenuItemIngredientRequest request, CancellationToken cancellationToken)
    {
        // Check if menu item exists
        var menuItemExists = await _db.MenuItems.AnyAsync(m => m.Id == request.MenuItemId, cancellationToken);
        if (!menuItemExists)
            return Results.NotFound(new { error = "Menu item not found" });

        // Check if inventory item exists
        var inventoryItemExists = await _db.InventoryItems.AnyAsync(i => i.Id == request.InventoryItemId, cancellationToken);
        if (!inventoryItemExists)
            return Results.NotFound(new { error = "Inventory item not found" });

        // Check if relationship already exists
        var exists = await _db.MenuItemIngredients
            .AnyAsync(mii => mii.MenuItemId == request.MenuItemId && mii.InventoryItemId == request.InventoryItemId, cancellationToken);

        if (exists)
            return Results.BadRequest(new { error = "This ingredient is already added to the menu item" });

        var ingredient = new MenuItemIngredient
        {
            MenuItemId = request.MenuItemId,
            InventoryItemId = request.InventoryItemId,
            QuantityRequired = request.QuantityRequired
        };

        _db.MenuItemIngredients.Add(ingredient);
        await _db.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/menu/{request.MenuItemId}/ingredients", new
        {
            ingredient.MenuItemId,
            ingredient.InventoryItemId,
            ingredient.QuantityRequired
        });
    }
}
