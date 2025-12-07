using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Menu;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace CampusEats.Tests.Features.Menu;

public class GetItemHandlerTests
{
    private CampusDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new CampusDbContext(options);
    }

    [Fact]
    public async Task Handle_WithValidId_ReturnsMenuItem()
    {
        // Arrange
        await using var context = CreateContext();
        var menuItemId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        
        var category = new Category
        {
            Id = categoryId,
            Name = "Burgers",
            IsActive = true
        };
        
        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Classic Burger",
            Description = "A delicious classic burger",
            Price = 9.99m,
            CategoryId = categoryId,
            ImageUrl = "https://example.com/burger.jpg",
            PreparationTimeMinutes = 15,
            IsAvailable = true,
            Calories = 650,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        context.Categories.Add(category);
        context.MenuItems.Add(menuItem);
        await context.SaveChangesAsync();
        
        var handler = new GetItemHandler(context);
        var request = new GetItemRequest(menuItemId);
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.ShouldBeOfType<Ok<GetItemResponse>>();
        var okResult = (Ok<GetItemResponse>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Id.ShouldBe(menuItemId);
        okResult.Value.Name.ShouldBe("Classic Burger");
        okResult.Value.Description.ShouldBe("A delicious classic burger");
        okResult.Value.Price.ShouldBe(9.99m);
        okResult.Value.CategoryName.ShouldBe("Burgers");
        okResult.Value.IsAvailable.ShouldBeTrue();
        okResult.Value.Calories.ShouldBe(650);
    }

    [Fact]
    public async Task Handle_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        await using var context = CreateContext();
        var handler = new GetItemHandler(context);
        var nonExistentId = Guid.NewGuid();
        var request = new GetItemRequest(nonExistentId);
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.ShouldBeOfType<NotFound<string>>();
        var notFoundResult = (NotFound<string>)result;
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ShouldContain(nonExistentId.ToString());
    }

    [Fact]
    public async Task Handle_WithAllergens_ReturnsMenuItemWithAllergens()
    {
        // Arrange
        await using var context = CreateContext();
        var menuItemId = Guid.NewGuid();
        var allergenId1 = Guid.NewGuid();
        var allergenId2 = Guid.NewGuid();
        
        var allergen1 = new Allergen
        {
            Id = allergenId1,
            Name = "Gluten",
            Description = "Contains gluten",
            Icon = "🌾"
        };
        
        var allergen2 = new Allergen
        {
            Id = allergenId2,
            Name = "Dairy",
            Description = "Contains dairy products",
            Icon = "🥛"
        };
        
        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Cheese Pizza",
            Description = "Pizza with cheese",
            Price = 12.99m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        context.Allergens.AddRange(allergen1, allergen2);
        context.MenuItems.Add(menuItem);
        await context.SaveChangesAsync();
        
        context.MenuItemAllergens.AddRange(
            new MenuItemAllergen { MenuItemId = menuItemId, AllergenId = allergenId1 },
            new MenuItemAllergen { MenuItemId = menuItemId, AllergenId = allergenId2 }
        );
        await context.SaveChangesAsync();
        
        var handler = new GetItemHandler(context);
        var request = new GetItemRequest(menuItemId);
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.ShouldBeOfType<Ok<GetItemResponse>>();
        var okResult = (Ok<GetItemResponse>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Allergens.ShouldNotBeNull();
        okResult.Value.Allergens.Count.ShouldBe(2);
        okResult.Value.Allergens.ShouldContain(a => a.Name == "Gluten");
        okResult.Value.Allergens.ShouldContain(a => a.Name == "Dairy");
    }

    [Fact]
    public async Task Handle_WithDietaryRestrictions_ReturnsMenuItemWithDietaryRestrictions()
    {
        // Arrange
        await using var context = CreateContext();
        var menuItemId = Guid.NewGuid();
        var restrictionId = Guid.NewGuid();
        
        var restriction = new DietaryRestriction
        {
            Id = restrictionId,
            Name = "Vegan",
            Description = "No animal products",
            Icon = "🌱"
        };
        
        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Vegan Salad",
            Description = "Fresh garden salad",
            Price = 8.99m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        context.DietaryRestrictions.Add(restriction);
        context.MenuItems.Add(menuItem);
        await context.SaveChangesAsync();
        
        context.MenuItemDietaryRestrictions.Add(
            new MenuItemDietaryRestriction { MenuItemId = menuItemId, DietaryRestrictionId = restrictionId }
        );
        await context.SaveChangesAsync();
        
        var handler = new GetItemHandler(context);
        var request = new GetItemRequest(menuItemId);
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.ShouldBeOfType<Ok<GetItemResponse>>();
        var okResult = (Ok<GetItemResponse>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.DietaryRestrictions.ShouldNotBeNull();
        okResult.Value.DietaryRestrictions.Count.ShouldBe(1);
        okResult.Value.DietaryRestrictions[0].Name.ShouldBe("Vegan");
    }
}
