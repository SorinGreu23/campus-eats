using CampusEats.Api.Features.Menu;
using FluentValidation.TestHelper;
using Shouldly;

namespace CampusEats.Tests.Features.Menu;

public class UpdateItemValidatorTests
{
    private readonly UpdateItemValidator _validator = new();

    [Fact]
    public void GivenValidRequest_WhenValidating_ThenPassesValidation()
    {
        // Arrange
        var request = new UpdateItemRequest(
            Name: "Valid Item",
            Description: "Valid description",
            Price: 10.99m,
            CategoryId: Guid.NewGuid(),
            ImageUrl: "https://example.com/image.jpg",
            PreparationTimeMinutes: 15,
            IsAvailable: true,
            Calories: 500,
            AllergenIds: null,
            DietaryRestrictionIds: null
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void GivenEmptyName_WhenValidating_ThenFailsValidation()
    {
        // Arrange
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

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required");
    }

    [Fact]
    public void Validate_WithNameTooLong_FailsValidation()
    {
        // Arrange
        var longName = new string('a', 101);
        var request = new UpdateItemRequest(
            Name: longName,
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

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not exceed 100 characters");
    }

    [Fact]
    public void GivenZeroPrice_WhenValidating_ThenFailsValidation()
    {
        // Arrange
        var request = new UpdateItemRequest(
            Name: "Test Item",
            Description: null,
            Price: 0m,
            CategoryId: null,
            ImageUrl: null,
            PreparationTimeMinutes: null,
            IsAvailable: true,
            Calories: null,
            AllergenIds: null,
            DietaryRestrictionIds: null
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("Price must be greater than 0");
    }

    [Fact]
    public void GivenNegativePrice_WhenValidating_ThenFailsValidation()
    {
        // Arrange
        var request = new UpdateItemRequest(
            Name: "Test Item",
            Description: null,
            Price: -5.99m,
            CategoryId: null,
            ImageUrl: null,
            PreparationTimeMinutes: null,
            IsAvailable: true,
            Calories: null,
            AllergenIds: null,
            DietaryRestrictionIds: null
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void GivenNegativePreparationTime_WhenValidating_ThenFailsValidation()
    {
        // Arrange
        var request = new UpdateItemRequest(
            Name: "Test Item",
            Description: null,
            Price: 10.99m,
            CategoryId: null,
            ImageUrl: null,
            PreparationTimeMinutes: -5,
            IsAvailable: true,
            Calories: null,
            AllergenIds: null,
            DietaryRestrictionIds: null
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PreparationTimeMinutes)
            .WithErrorMessage("Preparation time must be non-negative");
    }

    [Fact]
    public void GivenNegativeCalories_WhenValidating_ThenFailsValidation()
    {
        // Arrange
        var request = new UpdateItemRequest(
            Name: "Test Item",
            Description: null,
            Price: 10.99m,
            CategoryId: null,
            ImageUrl: null,
            PreparationTimeMinutes: null,
            IsAvailable: true,
            Calories: -100,
            AllergenIds: null,
            DietaryRestrictionIds: null
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Calories)
            .WithErrorMessage("Calories must be non-negative");
    }

    [Fact]
    public void GivenNullOptionalFields_WhenValidating_ThenPassesValidation()
    {
        // Arrange
        var request = new UpdateItemRequest(
            Name: "Simple Item",
            Description: null,
            Price: 5.99m,
            CategoryId: null,
            ImageUrl: null,
            PreparationTimeMinutes: null,
            IsAvailable: true,
            Calories: null,
            AllergenIds: null,
            DietaryRestrictionIds: null
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
