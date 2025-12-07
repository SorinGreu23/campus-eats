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
    public async Task Handle_WithNoItems_ReturnsEmptyList()
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
    public async Task Handle_WithMultipleItems_ReturnsAllItems()
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
    public async Task Handle_WithCategories_ReturnsItemsWithCategoryNames()
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
    public async Task Handle_WithAllergensAndDietaryRestrictions_ReturnsCompleteData()
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
        okResult.Value[0].Allergens.Count.ShouldBe(1);
        okResult.Value[0].Allergens[0].Name.ShouldBe("Nuts");
        okResult.Value[0].DietaryRestrictions.ShouldNotBeNull();
        okResult.Value[0].DietaryRestrictions.Count.ShouldBe(1);
        okResult.Value[0].DietaryRestrictions[0].Name.ShouldBe("Vegetarian");
    }
}
