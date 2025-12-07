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
    private CampusDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new CampusDbContext(options);
    }

    [Fact]
    public async Task Handle_WithValidRequest_CreatesMenuItem()
    {
        // Arrange
        await using var context = CreateContext();
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
    public async Task Handle_WithValidationErrors_ReturnsBadRequest()
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
    public async Task Handle_WithAllergens_CreatesMenuItemWithAllergens()
    {
        // Arrange
        await using var context = CreateContext();
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
    public async Task Handle_WithDietaryRestrictions_CreatesMenuItemWithDietaryRestrictions()
    {
        // Arrange
        await using var context = CreateContext();
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
    public async Task Handle_WithCategory_CreatesMenuItemWithCategory()
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
        createdResult.Value.CategoryId.ShouldBe(categoryId);
        
        var savedItem = await context.MenuItems.FirstOrDefaultAsync();
        savedItem.ShouldNotBeNull();
        savedItem.CategoryId.ShouldBe(categoryId);
    }
}
