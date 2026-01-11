using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Menu;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using FluentValidation.Results;

namespace CampusEats.Tests.Features.Menu;

public class CreateItemHandlerTests
{
    private static CampusDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new CampusDbContext(options);
    }

    [Fact]
    public async Task GivenValidRequest_WhenHandleIsCalled_ThenCreatesMenuItem()
    {
        // Arrange
        await using var context = CreateContext();
        var category = new Category { Id = Guid.NewGuid(), Name = "Test Category", DisplayOrder = 1 };
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        
        var validator = Substitute.For<IValidator<CreateItemRequest>>();
        validator.ValidateAsync(Arg.Any<CreateItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        
        var handler = new CreateItemHandler(context, validator);
        var request = new CreateItemRequest(
            Name: "New Burger",
            Description: "A new burger",
            Price: 11.99m,
            CategoryId: null,
            ImageUrl: "https://example.com/burger.jpg",
            PreparationTimeMinutes: 20,
            IsAvailable: true,
            Calories: 700,
            AllergenIds: null,
            DietaryRestrictionIds: null
        );
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.ShouldBeOfType<Created<CreateItemResponse>>();
        var createdResult = (Created<CreateItemResponse>)result;
        createdResult.Value.ShouldNotBeNull();
        createdResult.Value.Name.ShouldBe("New Burger");
        createdResult.Value.Price.ShouldBe(11.99m);
        
        var savedItem = await context.MenuItems.FirstOrDefaultAsync();
        savedItem.ShouldNotBeNull();
        savedItem.Name.ShouldBe("New Burger");
    }

    [Fact]
    public async Task GivenValidationErrors_WhenHandleIsCalled_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var validator = Substitute.For<IValidator<CreateItemRequest>>();
        
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name is required"),
            new ValidationFailure("Price", "Price must be greater than 0")
        };
        
        validator.ValidateAsync(Arg.Any<CreateItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(validationFailures));
        
        var handler = new CreateItemHandler(context, validator);
        var request = new CreateItemRequest(
            Name: "",
            Description: null,
            Price: -1m,
            CategoryId: null,
            ImageUrl: null,
            PreparationTimeMinutes: null,
            IsAvailable: true,
            Calories: null,
            AllergenIds: null,
            DietaryRestrictionIds: null
        );
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert - validation failed, so it should return BadRequest
        result.ShouldNotBeOfType<Created<CreateItemResponse>>();
        
        var itemCount = await context.MenuItems.CountAsync();
        itemCount.ShouldBe(0);
    }

    [Fact]
    public async Task GivenAllergenIds_WhenHandleIsCalled_ThenCreatesMenuItemWithAllergens()
    {
        // Arrange
        await using var context = CreateContext();
        var category = new Category { Id = Guid.NewGuid(), Name = "Test Category", DisplayOrder = 1 };
        context.Categories.Add(category);
        
        var allergenId1 = Guid.NewGuid();
        var allergenId2 = Guid.NewGuid();
        
        var allergen1 = new Allergen { Id = allergenId1, Name = "Gluten" };
        var allergen2 = new Allergen { Id = allergenId2, Name = "Dairy" };
        context.Allergens.AddRange(allergen1, allergen2);
        await context.SaveChangesAsync();
        
        var validator = Substitute.For<IValidator<CreateItemRequest>>();
        validator.ValidateAsync(Arg.Any<CreateItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        
        var handler = new CreateItemHandler(context, validator);
        var request = new CreateItemRequest(
            Name: "Pizza",
            Description: "Cheese pizza",
            Price: 12.99m,
            CategoryId: null,
            ImageUrl: null,
            PreparationTimeMinutes: null,
            IsAvailable: true,
            Calories: null,
            AllergenIds: new List<Guid> { allergenId1, allergenId2 },
            DietaryRestrictionIds: null
        );
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.ShouldBeOfType<Created<CreateItemResponse>>();
        var createdResult = (Created<CreateItemResponse>)result;
        createdResult.Value.ShouldNotBeNull();
        createdResult.Value.AllergenIds.ShouldNotBeNull();
        createdResult.Value.AllergenIds.Count.ShouldBe(2);
        
        var savedItem = await context.MenuItems
            .Include(m => m.MenuItemAllergens)
            .FirstOrDefaultAsync();
        savedItem.ShouldNotBeNull();
        savedItem.MenuItemAllergens.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GivenDietaryRestrictionIds_WhenHandleIsCalled_ThenCreatesMenuItemWithDietaryRestrictions()
    {
        // Arrange
        await using var context = CreateContext();
        var category = new Category { Id = Guid.NewGuid(), Name = "Test Category", DisplayOrder = 1 };
        context.Categories.Add(category);
        
        var restrictionId = Guid.NewGuid();
        
        var restriction = new DietaryRestriction { Id = restrictionId, Name = "Vegan" };
        context.DietaryRestrictions.Add(restriction);
        await context.SaveChangesAsync();
        
        var validator = Substitute.For<IValidator<CreateItemRequest>>();
        validator.ValidateAsync(Arg.Any<CreateItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        
        var handler = new CreateItemHandler(context, validator);
        var request = new CreateItemRequest(
            Name: "Vegan Salad",
            Description: "Fresh salad",
            Price: 8.99m,
            CategoryId: null,
            ImageUrl: null,
            PreparationTimeMinutes: null,
            IsAvailable: true,
            Calories: null,
            AllergenIds: null,
            DietaryRestrictionIds: new List<Guid> { restrictionId }
        );
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.ShouldBeOfType<Created<CreateItemResponse>>();
        var createdResult = (Created<CreateItemResponse>)result;
        createdResult.Value.ShouldNotBeNull();
        createdResult.Value.DietaryRestrictionIds.ShouldNotBeNull();
        createdResult.Value.DietaryRestrictionIds.Count.ShouldBe(1);
        
        var savedItem = await context.MenuItems
            .Include(m => m.MenuItemDietaryRestrictions)
            .FirstOrDefaultAsync();
        savedItem.ShouldNotBeNull();
        savedItem.MenuItemDietaryRestrictions.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GivenCategoryId_WhenHandleIsCalled_ThenCreatesMenuItemWithCategory()
    {
        // Arrange
        await using var context = CreateContext();
        var categoryId = Guid.NewGuid();
        
        var category = new Category { Id = categoryId, Name = "Desserts" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        
        var validator = Substitute.For<IValidator<CreateItemRequest>>();
        validator.ValidateAsync(Arg.Any<CreateItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        
        var handler = new CreateItemHandler(context, validator);
        var request = new CreateItemRequest(
            Name: "Ice Cream",
            Description: "Vanilla ice cream",
            Price: 4.99m,
            CategoryId: categoryId,
            ImageUrl: null,
            PreparationTimeMinutes: 5,
            IsAvailable: true,
            Calories: 250,
            AllergenIds: null,
            DietaryRestrictionIds: null
        );
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.ShouldBeOfType<Created<CreateItemResponse>>();
        var createdResult = (Created<CreateItemResponse>)result;
        createdResult.Value!.CategoryId.ShouldBe(categoryId);
        
        var savedItem = await context.MenuItems.FirstOrDefaultAsync();
        savedItem.ShouldNotBeNull();
        savedItem.CategoryId.ShouldBe(categoryId);
    }

    [Fact]
    public async Task GivenItemWithAllergens_WhenCreating_ThenAssociatesAllergens()
    {
        // Arrange
        await using var context = CreateContext();
        
        // Add a category for the menu item
        var category = new Category { Id = Guid.NewGuid(), Name = "Test Category", IsActive = true };
        context.Categories.Add(category);
        
        var allergenId1 = Guid.NewGuid();
        var allergenId2 = Guid.NewGuid();
        var allergenId3 = Guid.NewGuid();
        
        var allergen1 = new Allergen { Id = allergenId1, Name = "Peanuts", Description = "Peanut allergen" };
        var allergen2 = new Allergen { Id = allergenId2, Name = "Tree Nuts", Description = "Tree nut allergen" };
        var allergen3 = new Allergen { Id = allergenId3, Name = "Soy", Description = "Soy allergen" };
        context.Allergens.AddRange(allergen1, allergen2, allergen3);
        await context.SaveChangesAsync();
        
        var validator = Substitute.For<IValidator<CreateItemRequest>>();
        validator.ValidateAsync(Arg.Any<CreateItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        
        var handler = new CreateItemHandler(context, validator);
        var request = new CreateItemRequest(
            Name: "Peanut Butter Cookie",
            Description: "Cookie with peanuts",
            Price: 3.99m,
            CategoryId: null,
            ImageUrl: null,
            PreparationTimeMinutes: 5,
            IsAvailable: true,
            Calories: 350,
            AllergenIds: new List<Guid> { allergenId1, allergenId2, allergenId3 },
            DietaryRestrictionIds: null
        );
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.ShouldBeOfType<Created<CreateItemResponse>>();
        var createdResult = (Created<CreateItemResponse>)result;
        createdResult.Value.ShouldNotBeNull();
        createdResult.Value.AllergenIds.ShouldNotBeNull();
        createdResult.Value.AllergenIds.Count.ShouldBe(3);
        createdResult.Value.AllergenIds.ShouldContain(allergenId1);
        createdResult.Value.AllergenIds.ShouldContain(allergenId2);
        createdResult.Value.AllergenIds.ShouldContain(allergenId3);
        
        var savedItem = await context.MenuItems
            .Include(m => m.MenuItemAllergens)
            .ThenInclude(mia => mia.Allergen)
            .FirstOrDefaultAsync();
        savedItem.ShouldNotBeNull();
        savedItem.MenuItemAllergens.Count.ShouldBe(3);
        savedItem.MenuItemAllergens.ShouldContain(mia => mia.AllergenId == allergenId1);
        savedItem.MenuItemAllergens.ShouldContain(mia => mia.AllergenId == allergenId2);
        savedItem.MenuItemAllergens.ShouldContain(mia => mia.AllergenId == allergenId3);
    }

    [Fact]
    public async Task GivenItemWithDietaryRestrictions_WhenCreating_ThenAssociatesDietaryRestrictions()
    {
        // Arrange
        await using var context = CreateContext();
                // Add a category for the menu item
        var category = new Category { Id = Guid.NewGuid(), Name = "Test Category", IsActive = true };
        context.Categories.Add(category);
                var restrictionId1 = Guid.NewGuid();
        var restrictionId2 = Guid.NewGuid();
        
        var restriction1 = new DietaryRestriction { Id = restrictionId1, Name = "Vegetarian", Description = "No meat" };
        var restriction2 = new DietaryRestriction { Id = restrictionId2, Name = "Gluten-Free", Description = "No gluten" };
        context.DietaryRestrictions.AddRange(restriction1, restriction2);
        await context.SaveChangesAsync();
        
        var validator = Substitute.For<IValidator<CreateItemRequest>>();
        validator.ValidateAsync(Arg.Any<CreateItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        
        var handler = new CreateItemHandler(context, validator);
        var request = new CreateItemRequest(
            Name: "Quinoa Bowl",
            Description: "Healthy quinoa bowl",
            Price: 12.99m,
            CategoryId: null,
            ImageUrl: null,
            PreparationTimeMinutes: 15,
            IsAvailable: true,
            Calories: 450,
            AllergenIds: null,
            DietaryRestrictionIds: new List<Guid> { restrictionId1, restrictionId2 }
        );
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.ShouldBeOfType<Created<CreateItemResponse>>();
        var createdResult = (Created<CreateItemResponse>)result;
        createdResult.Value.ShouldNotBeNull();
        createdResult.Value.DietaryRestrictionIds.ShouldNotBeNull();
        createdResult.Value.DietaryRestrictionIds.Count.ShouldBe(2);
        createdResult.Value.DietaryRestrictionIds.ShouldContain(restrictionId1);
        createdResult.Value.DietaryRestrictionIds.ShouldContain(restrictionId2);
        
        var savedItem = await context.MenuItems
            .Include(m => m.MenuItemDietaryRestrictions)
            .ThenInclude(midr => midr.DietaryRestriction)
            .FirstOrDefaultAsync();
        savedItem.ShouldNotBeNull();
        savedItem.MenuItemDietaryRestrictions.Count.ShouldBe(2);
        savedItem.MenuItemDietaryRestrictions.ShouldContain(midr => midr.DietaryRestrictionId == restrictionId1);
        savedItem.MenuItemDietaryRestrictions.ShouldContain(midr => midr.DietaryRestrictionId == restrictionId2);
    }

    [Fact]
    public async Task GivenInvalidCategoryId_WhenCreating_ThenValidationFails()
    {
        // Arrange
        await using var context = CreateContext();
        var nonExistentCategoryId = Guid.NewGuid();
        
        var validator = Substitute.For<IValidator<CreateItemRequest>>();
        
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("CategoryId", "Category does not exist")
        };
        
        validator.ValidateAsync(Arg.Any<CreateItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(validationFailures));
        
        var handler = new CreateItemHandler(context, validator);
        var request = new CreateItemRequest(
            Name: "Test Item",
            Description: "Item with invalid category",
            Price: 9.99m,
            CategoryId: nonExistentCategoryId,
            ImageUrl: null,
            PreparationTimeMinutes: 10,
            IsAvailable: true,
            Calories: 300,
            AllergenIds: null,
            DietaryRestrictionIds: null
        );
        
        // Act
        var result = await handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.ShouldNotBeOfType<Created<CreateItemResponse>>();
        
        var itemCount = await context.MenuItems.CountAsync();
        itemCount.ShouldBe(0);
    }
}
