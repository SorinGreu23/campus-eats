using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Menu.Ingredients;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Tests.Features.Menu.Ingredients;

public class AddMenuItemIngredientHandlerTests
{
    private static CampusDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CampusDbContext(options);
    }

    [Fact]
    public async Task GivenValidInputs_WhenAddingIngredient_ThenCreatesLinkAndReturnsCreated()
    {
        await using var db = CreateDbContext();
        var handler = new AddMenuItemIngredientHandler(db);

        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = "Burger",
            Price = 9.99m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        var inventoryItem = new InventoryItem
        {
            Id = Guid.NewGuid(),
            Name = "Beef",
            Unit = "kg",
            CurrentQuantity = 50,
            MinimumQuantity = 5,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.MenuItems.Add(menuItem);
        db.InventoryItems.Add(inventoryItem);
        await db.SaveChangesAsync();

        var request = new AddMenuItemIngredientRequest(menuItem.Id, inventoryItem.Id, 0.25m);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().BeAssignableTo<IResult>();
        result.GetType().Name.Should().StartWith("Created");

        var locationProp = result.GetType().GetProperty("Location");
        locationProp.Should().NotBeNull();
        var location = locationProp!.GetValue(result) as string;
        location.Should().Be($"/api/menu/{menuItem.Id}/ingredients");

        var link = await db.MenuItemIngredients.FirstOrDefaultAsync(i => i.MenuItemId == menuItem.Id && i.InventoryItemId == inventoryItem.Id);
        link.Should().NotBeNull();
        link!.QuantityRequired.Should().Be(0.25m);
    }

    [Fact]
    public async Task GivenDuplicateIngredient_WhenAdding_ThenReturnsBadRequest()
    {
        await using var db = CreateDbContext();
        var handler = new AddMenuItemIngredientHandler(db);

        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = "Burger",
            Price = 9.99m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        var inventoryItem = new InventoryItem
        {
            Id = Guid.NewGuid(),
            Name = "Beef",
            Unit = "kg",
            CurrentQuantity = 50,
            MinimumQuantity = 5,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        var existing = new MenuItemIngredient
        {
            MenuItemId = menuItem.Id,
            InventoryItemId = inventoryItem.Id,
            QuantityRequired = 0.25m
        };
        db.MenuItems.Add(menuItem);
        db.InventoryItems.Add(inventoryItem);
        db.MenuItemIngredients.Add(existing);
        await db.SaveChangesAsync();

        var request = new AddMenuItemIngredientRequest(menuItem.Id, inventoryItem.Id, 0.25m);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().BeAssignableTo<IResult>();
        result.GetType().Name.Should().StartWith("BadRequest");
    }

    [Fact]
    public async Task GivenMissingMenuItem_WhenAdding_ThenReturnsNotFound()
    {
        await using var db = CreateDbContext();
        var handler = new AddMenuItemIngredientHandler(db);

        var inventoryItem = new InventoryItem
        {
            Id = Guid.NewGuid(),
            Name = "Beef",
            Unit = "kg",
            CurrentQuantity = 50,
            MinimumQuantity = 5,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.InventoryItems.Add(inventoryItem);
        await db.SaveChangesAsync();

        var request = new AddMenuItemIngredientRequest(Guid.NewGuid(), inventoryItem.Id, 0.25m);

        var result = await handler.Handle(request, CancellationToken.None);

        result.GetType().Name.Should().StartWith("NotFound");
    }

    [Fact]
    public async Task GivenMissingInventoryItem_WhenAdding_ThenReturnsNotFound()
    {
        await using var db = CreateDbContext();
        var handler = new AddMenuItemIngredientHandler(db);

        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = "Burger",
            Price = 9.99m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.MenuItems.Add(menuItem);
        await db.SaveChangesAsync();

        var request = new AddMenuItemIngredientRequest(menuItem.Id, Guid.NewGuid(), 0.25m);

        var result = await handler.Handle(request, CancellationToken.None);

        result.GetType().Name.Should().StartWith("NotFound");
    }
}
