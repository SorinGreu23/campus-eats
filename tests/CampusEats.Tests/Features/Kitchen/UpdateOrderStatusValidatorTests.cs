using CampusEats.Api.Features.Kitchen;
using CampusEats.Api.Features.Orders;
using CampusEats.Api.Features.Orders.UpdateStatus;
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
        var command = new UpdateOrderStatusRequest(Guid.Empty, nameof(OrderStatus.Preparing));
        
        // Act
        var result = await _validator.ValidateAsync(command);
        
        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "OrderId");
    }
    
    [Fact]
    public async Task ShouldHaveError_WhenStatusIsInvalid()
    {
        // Arrange
        var command = new UpdateOrderStatusRequest(Guid.NewGuid(), nameof(OrderStatus.Pending));
        
        // Act
        var result = await _validator.ValidateAsync(command);
        
        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Status");
    }
    
    [Theory]
    [InlineData(OrderStatus.Preparing)]
    [InlineData(OrderStatus.Ready)]
    [InlineData(OrderStatus.Completed)]
    public async Task ShouldNotHaveError_WhenStatusIsValid(OrderStatus status)
    {
        // Arrange
        var command = new UpdateOrderStatusRequest(Guid.NewGuid(), status.ToString());
        
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
        var command = new UpdateOrderStatusRequest(Guid.NewGuid(), nameof(OrderStatus.Preparing));
        
        // Act
        var result = await _validator.ValidateAsync(command);
        
        // Assert
        result.IsValid.ShouldBeTrue();
    }
}

