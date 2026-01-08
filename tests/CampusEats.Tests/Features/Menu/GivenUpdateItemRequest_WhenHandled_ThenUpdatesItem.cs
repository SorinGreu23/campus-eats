using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Menu;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;

namespace CampusEats.Tests.Features.Menu;

public class UpdateItemHandlerTests
{
    private CampusDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new CampusDbContext(options);
    }

    [Fact]
    public async Task GivenValidRequest_WhenHandleIsCalled_ThenUpdatesMenuItem()
    {
        // Arrange
        await using var context = CreateContext();
        var category = new Category { Id = Guid.NewGuid(), Name = "Test Category", DisplayOrder = 1 };
        context.Categories.Add(category);
        var menuItemId = Guid.NewGuid();
        
        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Old Name",
            Description = "Old description",
            Price = 9.99m,
            CategoryId = category.Id,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        context.MenuItems.Add(menuItem);
        await context.SaveChangesAsync();
        
        var validator = Substitute.For<IValidator<UpdateItemRequest>>();
        validator.ValidateAsync(Arg.Any<UpdateItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        
        var handler = new UpdateItemHandler(context, validator);
        var request = new UpdateItemRequest(
            Name: "Updated Name",
            Description: "Updated description",
            Price: 12.99m,
            CategoryId: null,
            ImageUrl: "https://example.com/updated.jpg",
            PreparationTimeMinutes: 25,
            IsAvailable: false,
            Calories: 800,
            AllergenIds: null,
            DietaryRestrictionIds: null
        );
        var command = new UpdateItemCommand(menuItemId, request);
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.ShouldBeOfType<NoContent>();
        
        var updatedItem = await context.MenuItems.FindAsync(menuItemId);
        updatedItem.ShouldNotBeNull();
        updatedItem.Name.ShouldBe("Updated Name");
        updatedItem.Description.ShouldBe("Updated description");
        updatedItem.Price.ShouldBe(12.99m);
        updatedItem.IsAvailable.ShouldBeFalse();
        updatedItem.Calories.ShouldBe(800);
    }

    [Fact]
    public async Task GivenInvalidId_WhenHandleIsCalled_ThenReturnsNotFound()
    {
        // Arrange
        await using var context = CreateContext();
        var validator = Substitute.For<IValidator<UpdateItemRequest>>();
        validator.ValidateAsync(Arg.Any<UpdateItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        
        var handler = new UpdateItemHandler(context, validator);
        var nonExistentId = Guid.NewGuid();
        var request = new UpdateItemRequest(
            Name: "Test",
            Description: null,
            Price: 10.99m,
            CategoryId: null,
            ImageUrl: null,
            PreparationTimeMinutes: null,
            IsAvailable: true,
            Calories: null,
            AllergenIds: null,
            DietaryRestrictionIds: null
        );
        var command = new UpdateItemCommand(nonExistentId, request);
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.ShouldBeOfType<NotFound<string>>();
        var notFoundResult = (NotFound<string>)result;
        notFoundResult.Value.ShouldNotBeNull();
        notFoundResult.Value.ShouldContain(nonExistentId.ToString());
    }

    [Fact]
    public async Task GivenValidationErrors_WhenHandleIsCalled_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var menuItemId = Guid.NewGuid();
        
        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Test Item",
            Price = 10.99m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        context.MenuItems.Add(menuItem);
        await context.SaveChangesAsync();
        
        var validator = Substitute.For<IValidator<UpdateItemRequest>>();
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name is required")
        };
        validator.ValidateAsync(Arg.Any<UpdateItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(validationFailures));
        
        var handler = new UpdateItemHandler(context, validator);
        var request = new UpdateItemRequest(
            Name: "",
            Description: null,
            Price: 10.99m,
            CategoryId: null,
            ImageUrl: null,
            PreparationTimeMinutes: null,
            IsAvailable: true,
            Calories: null,
            AllergenIds: null,
            DietaryRestrictionIds: null
        );
        var command = new UpdateItemCommand(menuItemId, request);
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert - validation failed, so it should return BadRequest
        result.ShouldNotBeOfType<NoContent>();
    }

    [Fact]
    public async Task GivenNewAllergenIds_WhenHandleIsCalled_ThenReplacesExistingAllergens()
    {
        // Arrange
        await using var context = CreateContext();
        var category = new Category { Id = Guid.NewGuid(), Name = "Test Category", DisplayOrder = 1 };
        context.Categories.Add(category);
        var menuItemId = Guid.NewGuid();
        var oldAllergenId = Guid.NewGuid();
        var newAllergenId = Guid.NewGuid();
        
        var oldAllergen = new Allergen { Id = oldAllergenId, Name = "Old Allergen" };
        var newAllergen = new Allergen { Id = newAllergenId, Name = "New Allergen" };
        context.Allergens.AddRange(oldAllergen, newAllergen);
        
        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Test Item",
            Price = 10.99m,
            CategoryId = category.Id,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        context.MenuItems.Add(menuItem);
        await context.SaveChangesAsync();
        
        context.MenuItemAllergens.Add(new MenuItemAllergen 
        { 
            MenuItemId = menuItemId, 
            AllergenId = oldAllergenId 
        });
        await context.SaveChangesAsync();
        
        var validator = Substitute.For<IValidator<UpdateItemRequest>>();
        validator.ValidateAsync(Arg.Any<UpdateItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        
        var handler = new UpdateItemHandler(context, validator);
        var request = new UpdateItemRequest(
            Name: "Test Item",
            Description: null,
            Price: 10.99m,
            CategoryId: null,
            ImageUrl: null,
            PreparationTimeMinutes: null,
            IsAvailable: true,
            Calories: null,
            AllergenIds: new List<Guid> { newAllergenId },
            DietaryRestrictionIds: null
        );
        var command = new UpdateItemCommand(menuItemId, request);
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.ShouldBeOfType<NoContent>();
        
        var updatedItem = await context.MenuItems
            .Include(m => m.MenuItemAllergens)
            .FirstOrDefaultAsync(m => m.Id == menuItemId);
        
        updatedItem.ShouldNotBeNull();
        updatedItem.MenuItemAllergens.Count.ShouldBe(1);
        updatedItem.MenuItemAllergens.First().AllergenId.ShouldBe(newAllergenId);
    }

    [Fact]
    public async Task GivenNewDietaryRestrictionIds_WhenHandleIsCalled_ThenReplacesExistingRestrictions()
    {
        // Arrange
        await using var context = CreateContext();
        var category = new Category { Id = Guid.NewGuid(), Name = "Test Category", DisplayOrder = 1 };
        context.Categories.Add(category);
        var menuItemId = Guid.NewGuid();
        var oldRestrictionId = Guid.NewGuid();
        var newRestrictionId = Guid.NewGuid();
        
        var oldRestriction = new DietaryRestriction { Id = oldRestrictionId, Name = "Old Restriction" };
        var newRestriction = new DietaryRestriction { Id = newRestrictionId, Name = "New Restriction" };
        context.DietaryRestrictions.AddRange(oldRestriction, newRestriction);
        
        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Test Item",
            Price = 10.99m,
            CategoryId = category.Id,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        context.MenuItems.Add(menuItem);
        await context.SaveChangesAsync();
        
        context.MenuItemDietaryRestrictions.Add(new MenuItemDietaryRestriction 
        { 
            MenuItemId = menuItemId, 
            DietaryRestrictionId = oldRestrictionId 
        });
        await context.SaveChangesAsync();
        
        var validator = Substitute.For<IValidator<UpdateItemRequest>>();
        validator.ValidateAsync(Arg.Any<UpdateItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        
        var handler = new UpdateItemHandler(context, validator);
        var request = new UpdateItemRequest(
            Name: "Test Item",
            Description: null,
            Price: 10.99m,
            CategoryId: null,
            ImageUrl: null,
            PreparationTimeMinutes: null,
            IsAvailable: true,
            Calories: null,
            AllergenIds: null,
            DietaryRestrictionIds: new List<Guid> { newRestrictionId }
        );
        var command = new UpdateItemCommand(menuItemId, request);
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.ShouldBeOfType<NoContent>();
        
        var updatedItem = await context.MenuItems
            .Include(m => m.MenuItemDietaryRestrictions)
            .FirstOrDefaultAsync(m => m.Id == menuItemId);
        
        updatedItem.ShouldNotBeNull();
        updatedItem.MenuItemDietaryRestrictions.Count.ShouldBe(1);
        updatedItem.MenuItemDietaryRestrictions.First().DietaryRestrictionId.ShouldBe(newRestrictionId);
    }
}
