using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Menu;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace CampusEats.Tests.Features.Menu;

public class DeleteItemHandlerTests
{
    private static CampusDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new CampusDbContext(options);
    }

    [Fact]
    public async Task GivenValidId_WhenHandleIsCalled_ThenDeletesMenuItem()
    {
        // Arrange
        await using var context = CreateContext();
        var menuItemId = Guid.NewGuid();
        
        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Item to Delete",
            Description = "This will be deleted",
            Price = 10.99m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        context.MenuItems.Add(menuItem);
        await context.SaveChangesAsync();
        
        var handler = new DeleteItemHandler(context);
        var request = new DeleteItemRequest(menuItemId);
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.ShouldBeOfType<NoContent>();
        
        var deletedItem = await context.MenuItems.FindAsync(menuItemId);
        deletedItem.ShouldBeNull();
    }

    [Fact]
    public async Task GivenInvalidId_WhenHandleIsCalled_ThenReturnsNotFound()
    {
        // Arrange
        await using var context = CreateContext();
        var handler = new DeleteItemHandler(context);
        var nonExistentId = Guid.NewGuid();
        var request = new DeleteItemRequest(nonExistentId);
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.ShouldBeOfType<NotFound<string>>();
        var notFoundResult = (NotFound<string>)result;
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ShouldContain(nonExistentId.ToString());
    }

    [Fact]
    public async Task GivenMultipleItems_WhenHandleIsCalled_ThenLeavesOtherItemsIntact()
    {
        // Arrange
        await using var context = CreateContext();
        var menuItemId1 = Guid.NewGuid();
        var menuItemId2 = Guid.NewGuid();
        
        var menuItem1 = new MenuItem
        {
            Id = menuItemId1,
            Name = "Item to Delete",
            Price = 10.99m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        var menuItem2 = new MenuItem
        {
            Id = menuItemId2,
            Name = "Item to Keep",
            Price = 12.99m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        context.MenuItems.AddRange(menuItem1, menuItem2);
        await context.SaveChangesAsync();
        
        var handler = new DeleteItemHandler(context);
        var request = new DeleteItemRequest(menuItemId1);
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.ShouldBeOfType<NoContent>();
        
        var deletedItem = await context.MenuItems.FindAsync(menuItemId1);
        deletedItem.ShouldBeNull();
        
        var remainingItem = await context.MenuItems.FindAsync(menuItemId2);
        remainingItem.ShouldNotBeNull();
        remainingItem.Name.ShouldBe("Item to Keep");
    }

    [Fact]
    public async Task GivenItemWithRelations_WhenHandleIsCalled_ThenCascadesCorrectly()
    {
        // Arrange
        await using var context = CreateContext();
        var menuItemId = Guid.NewGuid();
        var allergenId = Guid.NewGuid();
        var restrictionId = Guid.NewGuid();
        
        var allergen = new Allergen { Id = allergenId, Name = "Test Allergen" };
        var restriction = new DietaryRestriction { Id = restrictionId, Name = "Test Restriction" };
        
        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Complex Item",
            Price = 15.99m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        context.Allergens.Add(allergen);
        context.DietaryRestrictions.Add(restriction);
        context.MenuItems.Add(menuItem);
        await context.SaveChangesAsync();
        
        context.MenuItemAllergens.Add(new MenuItemAllergen 
        { 
            MenuItemId = menuItemId, 
            AllergenId = allergenId 
        });
        context.MenuItemDietaryRestrictions.Add(new MenuItemDietaryRestriction 
        { 
            MenuItemId = menuItemId, 
            DietaryRestrictionId = restrictionId 
        });
        await context.SaveChangesAsync();
        
        var handler = new DeleteItemHandler(context);
        var request = new DeleteItemRequest(menuItemId);
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.ShouldBeOfType<NoContent>();
        
        var deletedItem = await context.MenuItems.FindAsync(menuItemId);
        deletedItem.ShouldBeNull();
        
        // Verify cascade delete for join tables
        var allergenRelation = await context.MenuItemAllergens
            .FirstOrDefaultAsync(ma => ma.MenuItemId == menuItemId);
        allergenRelation.ShouldBeNull();
        
        var restrictionRelation = await context.MenuItemDietaryRestrictions
            .FirstOrDefaultAsync(mr => mr.MenuItemId == menuItemId);
        restrictionRelation.ShouldBeNull();
        
        // Verify that allergens and restrictions themselves are not deleted
        var allergenStillExists = await context.Allergens.FindAsync(allergenId);
        allergenStillExists.ShouldNotBeNull();
        
        var restrictionStillExists = await context.DietaryRestrictions.FindAsync(restrictionId);
        restrictionStillExists.ShouldNotBeNull();
    }
}
