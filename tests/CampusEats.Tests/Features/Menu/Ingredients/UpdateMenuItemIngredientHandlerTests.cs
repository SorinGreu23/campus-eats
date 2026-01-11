using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Menu.Ingredients;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Tests.Features.Menu.Ingredients;

public class UpdateMenuItemIngredientHandlerTests
{
    private static CampusDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CampusDbContext(options);
    }

    [Fact]
    public async Task GivenExistingLink_WhenHandled_ThenUpdatesQuantityAndReturnsOk()
    {
        await using var db = CreateDbContext();
        var handler = new UpdateMenuItemIngredientHandler(db);

        var menuItemId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();

        db.MenuItems.Add(new MenuItem
        {
            Id = menuItemId,
            Name = "Wrap",
            Price = 8.75m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        db.InventoryItems.Add(new InventoryItem
        {
            Id = inventoryItemId,
            Name = "Lettuce",
            Unit = "kg",
            CurrentQuantity = 4m,
            MinimumQuantity = 0.5m,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        db.MenuItemIngredients.Add(new MenuItemIngredient
        {
            MenuItemId = menuItemId,
            InventoryItemId = inventoryItemId,
            QuantityRequired = 0.3m
        });

        await db.SaveChangesAsync();

        var request = new UpdateMenuItemIngredientRequest(menuItemId, inventoryItemId, 0.6m);

        var result = await handler.Handle(request, CancellationToken.None);

        var statusResult = result as IStatusCodeHttpResult;
        statusResult.Should().NotBeNull();
        statusResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

        var valueResult = result as IValueHttpResult;
        valueResult.Should().NotBeNull();

        var json = JsonSerializer.Serialize(valueResult!.Value);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        root.GetProperty("MenuItemId").GetGuid().Should().Be(menuItemId);
        root.GetProperty("InventoryItemId").GetGuid().Should().Be(inventoryItemId);
        root.GetProperty("QuantityRequired").GetDecimal().Should().Be(0.6m);

        var updatedLink = await db.MenuItemIngredients.FirstAsync();
        updatedLink.QuantityRequired.Should().Be(0.6m);
    }

    [Fact]
    public async Task GivenMissingLink_WhenHandled_ThenReturnsNotFound()
    {
        await using var db = CreateDbContext();
        var handler = new UpdateMenuItemIngredientHandler(db);

        var menuItemId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();

        var result = await handler.Handle(new UpdateMenuItemIngredientRequest(menuItemId, inventoryItemId, 1.0m), CancellationToken.None);

        var statusResult = result as IStatusCodeHttpResult;
        statusResult.Should().NotBeNull();
        statusResult!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
}
