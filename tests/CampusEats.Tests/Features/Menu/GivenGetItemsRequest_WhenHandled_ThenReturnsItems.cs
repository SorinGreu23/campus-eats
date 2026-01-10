using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Menu;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace CampusEats.Tests.Features.Menu;

public class GetItemsHandlerTests
{
    private CampusDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new CampusDbContext(options);
    }

    [Fact]
    public async Task GivenNoItems_WhenHandleIsCalled_ThenReturnsEmptyList()
    {
        // Arrange
        await using var context = CreateContext();
        var handler = new GetItemsHandler(context);
        var request = new GetItemsRequest();
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.ShouldBeOfType<Ok<List<GetItemsResponse>>>();
        var okResult = (Ok<List<GetItemsResponse>>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GivenMultipleItems_WhenHandleIsCalled_ThenReturnsAllItems()
    {
        // Arrange
        await using var context = CreateContext();
        
        var menuItem1 = new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = "Burger",
            Description = "Tasty burger",
            Price = 10.99m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        var menuItem2 = new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = "Pizza",
            Description = "Cheesy pizza",
            Price = 12.99m,
            IsAvailable = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        context.MenuItems.AddRange(menuItem1, menuItem2);
        await context.SaveChangesAsync();
        
        var handler = new GetItemsHandler(context);
        var request = new GetItemsRequest();
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.ShouldBeOfType<Ok<List<GetItemsResponse>>>();
        var okResult = (Ok<List<GetItemsResponse>>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Count.ShouldBe(2);
        okResult.Value.ShouldContain(item => item.Name == "Burger");
        okResult.Value.ShouldContain(item => item.Name == "Pizza");
    }

    [Fact]
    public async Task GivenItemsWithCategories_WhenHandleIsCalled_ThenReturnsItemsWithCategoryNames()
    {
        // Arrange
        await using var context = CreateContext();
        var categoryId = Guid.NewGuid();
        
        var category = new Category
        {
            Id = categoryId,
            Name = "Fast Food",
            IsActive = true
        };
        
        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = "Hot Dog",
            Description = "Classic hot dog",
            Price = 5.99m,
            CategoryId = categoryId,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        context.Categories.Add(category);
        context.MenuItems.Add(menuItem);
        await context.SaveChangesAsync();
        
        var handler = new GetItemsHandler(context);
        var request = new GetItemsRequest();
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.ShouldBeOfType<Ok<List<GetItemsResponse>>>();
        var okResult = (Ok<List<GetItemsResponse>>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Count.ShouldBe(1);
        okResult.Value[0].CategoryName.ShouldBe("Fast Food");
    }

    [Fact]
    public async Task GivenItemsWithAllergensAndRestrictions_WhenHandleIsCalled_ThenReturnsCompleteData()
    {
        // Arrange
        await using var context = CreateContext();
        var menuItemId = Guid.NewGuid();
        var allergenId = Guid.NewGuid();
        var restrictionId = Guid.NewGuid();
        
        var allergen = new Allergen
        {
            Id = allergenId,
            Name = "Nuts",
            Description = "Tree nuts",
            Icon = "🥜"
        };
        
        var restriction = new DietaryRestriction
        {
            Id = restrictionId,
            Name = "Vegetarian",
            Description = "No meat",
            Icon = "🥗"
        };
        
        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Veggie Burger",
            Description = "Vegetarian burger with nuts",
            Price = 9.99m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        context.Allergens.Add(allergen);
        context.DietaryRestrictions.Add(restriction);
        context.MenuItems.Add(menuItem);
        await context.SaveChangesAsync();
        
        context.MenuItemAllergens.Add(new MenuItemAllergen { MenuItemId = menuItemId, AllergenId = allergenId });
        context.MenuItemDietaryRestrictions.Add(new MenuItemDietaryRestriction { MenuItemId = menuItemId, DietaryRestrictionId = restrictionId });
        await context.SaveChangesAsync();
        
        var handler = new GetItemsHandler(context);
        var request = new GetItemsRequest();
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.ShouldBeOfType<Ok<List<GetItemsResponse>>>();
        var okResult = (Ok<List<GetItemsResponse>>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Count.ShouldBe(1);
        okResult.Value[0].Allergens.ShouldNotBeNull();
        okResult.Value[0].Allergens!.Count.ShouldBe(1);
        okResult.Value[0].Allergens![0].Name.ShouldBe("Nuts");
        okResult.Value[0].DietaryRestrictions.ShouldNotBeNull();
        okResult.Value[0].DietaryRestrictions!.Count.ShouldBe(1);
        okResult.Value[0].DietaryRestrictions![0].Name.ShouldBe("Vegetarian");
    }

    [Fact]
    public async Task GivenItemWithInsufficientStock_WhenHandleIsCalled_ThenItemIsExcluded()
    {
        // Arrange
        await using var context = CreateContext();
        var menuItemId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();
        
        var inventoryItem = new InventoryItem
        {
            Id = inventoryItemId,
            Name = "Beef Patty",
            Unit = "kg",
            CurrentQuantity = 5m,
            MinimumQuantity = 2m,
            IsOutOfStock = false,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Burger",
            Description = "Beef burger",
            Price = 10.99m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        var menuItemIngredient = new MenuItemIngredient
        {
            MenuItemId = menuItemId,
            InventoryItemId = inventoryItemId,
            QuantityRequired = 10m // Requires 10kg but only 5kg available
        };
        
        context.InventoryItems.Add(inventoryItem);
        context.MenuItems.Add(menuItem);
        await context.SaveChangesAsync();
        
        context.MenuItemIngredients.Add(menuItemIngredient);
        await context.SaveChangesAsync();
        
        var handler = new GetItemsHandler(context);
        var request = new GetItemsRequest();
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.ShouldBeOfType<Ok<List<GetItemsResponse>>>();
        var okResult = (Ok<List<GetItemsResponse>>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Count.ShouldBe(0); // Item should be excluded due to insufficient stock
    }

    [Fact]
    public async Task GivenItemWithSufficientStock_WhenHandleIsCalled_ThenItemIsIncluded()
    {
        // Arrange
        await using var context = CreateContext();
        var menuItemId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();
        
        var inventoryItem = new InventoryItem
        {
            Id = inventoryItemId,
            Name = "Beef Patty",
            Unit = "kg",
            CurrentQuantity = 100m,
            MinimumQuantity = 10m,
            IsOutOfStock = false,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Burger",
            Description = "Beef burger",
            Price = 10.99m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        var menuItemIngredient = new MenuItemIngredient
        {
            MenuItemId = menuItemId,
            InventoryItemId = inventoryItemId,
            QuantityRequired = 0.25m // Requires 0.25kg and 100kg available
        };
        
        context.InventoryItems.Add(inventoryItem);
        context.MenuItems.Add(menuItem);
        await context.SaveChangesAsync();
        
        context.MenuItemIngredients.Add(menuItemIngredient);
        await context.SaveChangesAsync();
        
        var handler = new GetItemsHandler(context);
        var request = new GetItemsRequest();
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.ShouldBeOfType<Ok<List<GetItemsResponse>>>();
        var okResult = (Ok<List<GetItemsResponse>>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Count.ShouldBe(1);
        okResult.Value[0].Name.ShouldBe("Burger");
    }

    [Fact]
    public async Task GivenItemWithNoIngredients_WhenHandleIsCalled_ThenItemIsIncluded()
    {
        // Arrange
        await using var context = CreateContext();
        
        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = "Coffee",
            Description = "Black coffee (no inventory tracking)",
            Price = 2.99m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        context.MenuItems.Add(menuItem);
        await context.SaveChangesAsync();
        
        var handler = new GetItemsHandler(context);
        var request = new GetItemsRequest();
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.ShouldBeOfType<Ok<List<GetItemsResponse>>>();
        var okResult = (Ok<List<GetItemsResponse>>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Count.ShouldBe(1);
        okResult.Value[0].Name.ShouldBe("Coffee");
    }
}
