using System;
using System.Threading;
using System.Threading.Tasks;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Menu.Ingredients;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Tests.Features.Menu.Ingredients;

public class DeleteMenuItemIngredientHandlerTests
{
    private static CampusDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CampusDbContext(options);
    }

    [Fact]
    public async Task GivenExistingLink_WhenHandled_ThenDeletesAndReturnsNoContent()
    {
        await using var db = CreateDbContext();
        var handler = new DeleteMenuItemIngredientHandler(db);

        var menuItemId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();

        db.MenuItems.Add(new MenuItem
        {
            Id = menuItemId,
            Name = "Pizza",
            Price = 15m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        db.InventoryItems.Add(new InventoryItem
        {
            Id = inventoryItemId,
            Name = "Cheese",
            Unit = "kg",
            CurrentQuantity = 5m,
            MinimumQuantity = 1m,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        db.MenuItemIngredients.Add(new MenuItemIngredient
        {
            MenuItemId = menuItemId,
            InventoryItemId = inventoryItemId,
            QuantityRequired = 0.2m
        });

        await db.SaveChangesAsync();

        var result = await handler.Handle(new DeleteMenuItemIngredientRequest(menuItemId, inventoryItemId), CancellationToken.None);

        var statusResult = result as IStatusCodeHttpResult;
        statusResult.Should().NotBeNull();
        statusResult!.StatusCode.Should().Be(StatusCodes.Status204NoContent);

        (await db.MenuItemIngredients.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task GivenMissingLink_WhenHandled_ThenReturnsNotFound()
    {
        await using var db = CreateDbContext();
        var handler = new DeleteMenuItemIngredientHandler(db);

        var menuItemId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();

        var result = await handler.Handle(new DeleteMenuItemIngredientRequest(menuItemId, inventoryItemId), CancellationToken.None);

        var statusResult = result as IStatusCodeHttpResult;
        statusResult.Should().NotBeNull();
        statusResult!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
}
