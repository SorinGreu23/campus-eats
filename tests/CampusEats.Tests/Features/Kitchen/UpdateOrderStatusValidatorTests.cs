using CampusEats.Api.Features.Kitchen;
using FluentValidation;
using Shouldly;
using Xunit;

namespace CampusEats.Tests.Features.Kitchen;

public class UpdateOrderStatusValidatorTests
{
    private readonly UpdateOrderStatusValidator _validator = new();
    
    [Fact]
    public async Task ShouldHaveError_WhenOrderIdIsEmpty()
    {
        // Arrange
        var command = new UpdateOrderStatusCommand(Guid.Empty, "Preparing");
        
        // Act
        var result = await _validator.ValidateAsync(command);
        
        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "OrderId");
    }
    
    [Fact]
    public async Task ShouldHaveError_WhenStatusIsEmpty()
    {
        // Arrange
        var command = new UpdateOrderStatusCommand(Guid.NewGuid(), "");
        
        // Act
        var result = await _validator.ValidateAsync(command);
        
        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Status");
    }
    
    [Fact]
    public async Task ShouldHaveError_WhenStatusIsInvalid()
    {
        // Arrange
        var command = new UpdateOrderStatusCommand(Guid.NewGuid(), "InvalidStatus");
        
        // Act
        var result = await _validator.ValidateAsync(command);
        
        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Status");
    }
    
    [Theory]
    [InlineData("Preparing")]
    [InlineData("Ready")]
    [InlineData("Completed")]
    public async Task ShouldNotHaveError_WhenStatusIsValid(string status)
    {
        // Arrange
        var command = new UpdateOrderStatusCommand(Guid.NewGuid(), status);
        
        // Act
        var result = await _validator.ValidateAsync(command);
        
        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldNotContain(e => e.PropertyName == "Status");
    }
    
    [Fact]
    public async Task ShouldNotHaveError_WhenCommandIsValid()
    {
        // Arrange
        var command = new UpdateOrderStatusCommand(Guid.NewGuid(), "Preparing");
        
        // Act
        var result = await _validator.ValidateAsync(command);
        
        // Assert
        result.IsValid.ShouldBeTrue();
    }
}

