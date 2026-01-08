using CampusEats.Api.Features.Inventory.Restock;
using FluentValidation.TestHelper;

namespace CampusEats.Tests.Features.Inventory;

public class GivenRestockRequest_WhenValidating_ThenValidatesRules
{
    private readonly RestockInventoryValidator _validator;

    public GivenRestockRequest_WhenValidating_ThenValidatesRules()
    {
        _validator = new RestockInventoryValidator();
    }

    [Fact]
    public async Task GivenEmptyInventoryItemId_WhenValidating_ThenReturnsError()
    {
        // Arrange
        var request = new RestockInventoryRequest(Guid.Empty, 10m, "Test");

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.InventoryItemId)
            .WithErrorMessage("Inventory item ID is required");
    }

    [Fact]
    public async Task GivenZeroQuantity_WhenValidating_ThenReturnsError()
    {
        // Arrange
        var request = new RestockInventoryRequest(Guid.NewGuid(), 0m, "Test");

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity)
            .WithErrorMessage("Quantity must be greater than 0");
    }

    [Fact]
    public async Task GivenNegativeQuantity_WhenValidating_ThenReturnsError()
    {
        // Arrange
        var request = new RestockInventoryRequest(Guid.NewGuid(), -10m, "Test");

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity)
            .WithErrorMessage("Quantity must be greater than 0");
    }

    [Fact]
    public async Task GivenReasonExceeding500Characters_WhenValidating_ThenReturnsError()
    {
        // Arrange
        var longReason = new string('a', 501);
        var request = new RestockInventoryRequest(Guid.NewGuid(), 10m, longReason);

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage("Reason must not exceed 500 characters");
    }

    [Fact]
    public async Task GivenValidRequest_WhenValidating_ThenPassesValidation()
    {
        // Arrange
        var request = new RestockInventoryRequest(Guid.NewGuid(), 50m, "Weekly restock");

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task GivenNullReason_WhenValidating_ThenPassesValidation()
    {
        // Arrange
        var request = new RestockInventoryRequest(Guid.NewGuid(), 25m, null);

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public async Task GivenReasonExactly500Characters_WhenValidating_ThenPassesValidation()
    {
        // Arrange
        var maxLengthReason = new string('a', 500);
        var request = new RestockInventoryRequest(Guid.NewGuid(), 10m, maxLengthReason);

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public async Task GivenDecimalQuantity_WhenValidating_ThenPassesValidation()
    {
        // Arrange
        var request = new RestockInventoryRequest(Guid.NewGuid(), 10.5m, "Half kg restock");

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
