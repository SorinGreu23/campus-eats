using System;
using System.Collections.Generic;
using System.Linq;
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

public class GetMenuItemIngredientsHandlerTests
{
    private static CampusDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CampusDbContext(options);
    }

    [Fact]
    public async Task GivenMenuItemWithIngredients_WhenHandled_ThenReturnsProjectedList()
    {
        await using var db = CreateDbContext();
        var handler = new GetMenuItemIngredientsHandler(db);

        var menuItemId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();

        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Pasta",
            Price = 12.50m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var inventoryItem = new InventoryItem
        {
            Id = inventoryItemId,
            Name = "Tomato",
            Unit = "kg",
            CurrentQuantity = 8.5m,
            MinimumQuantity = 1.0m,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var link = new MenuItemIngredient
        {
            MenuItemId = menuItemId,
            InventoryItemId = inventoryItemId,
            QuantityRequired = 0.75m,
            InventoryItem = inventoryItem
        };

        db.MenuItems.Add(menuItem);
        db.InventoryItems.Add(inventoryItem);
        db.MenuItemIngredients.Add(link);
        await db.SaveChangesAsync();

        var request = new GetMenuItemIngredientsRequest(menuItemId);

        var result = await handler.Handle(request, CancellationToken.None);

        var statusResult = result as IStatusCodeHttpResult;
        statusResult.Should().NotBeNull();
        statusResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

        var valueResult = result as IValueHttpResult;
        valueResult.Should().NotBeNull();

        var serialized = JsonSerializer.Serialize(valueResult!.Value);
        using var doc = JsonDocument.Parse(serialized);
        var root = doc.RootElement;
        root.ValueKind.Should().Be(JsonValueKind.Array);
        root.GetArrayLength().Should().Be(1);

        var ingredient = root[0];
        ingredient.GetProperty("MenuItemId").GetGuid().Should().Be(menuItemId);
        ingredient.GetProperty("InventoryItemId").GetGuid().Should().Be(inventoryItemId);
        ingredient.GetProperty("QuantityRequired").GetDecimal().Should().Be(0.75m);

        var inventory = ingredient.GetProperty("InventoryItem");
        inventory.GetProperty("Id").GetGuid().Should().Be(inventoryItemId);
        inventory.GetProperty("Name").GetString().Should().Be("Tomato");
        inventory.GetProperty("Unit").GetString().Should().Be("kg");
        inventory.GetProperty("CurrentQuantity").GetDecimal().Should().Be(8.5m);
        inventory.GetProperty("MinimumQuantity").GetDecimal().Should().Be(1.0m);
    }

    [Fact]
    public async Task GivenMenuItemWithoutIngredients_WhenHandled_ThenReturnsEmptyList()
    {
        await using var db = CreateDbContext();
        var handler = new GetMenuItemIngredientsHandler(db);

        var menuItemId = Guid.NewGuid();
        db.MenuItems.Add(new MenuItem
        {
            Id = menuItemId,
            Name = "Salad",
            Price = 7.25m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var result = await handler.Handle(new GetMenuItemIngredientsRequest(menuItemId), CancellationToken.None);

        var statusResult = result as IStatusCodeHttpResult;
        statusResult.Should().NotBeNull();
        statusResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

        var valueResult = result as IValueHttpResult;
        valueResult.Should().NotBeNull();

        var serialized = JsonSerializer.Serialize(valueResult!.Value);
        using var doc = JsonDocument.Parse(serialized);
        doc.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
        doc.RootElement.GetArrayLength().Should().Be(0);
    }
}
