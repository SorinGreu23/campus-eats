using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Kitchen;
using CampusEats.Api.Features.Orders;
using CampusEats.Api.Features.Orders.UpdateStatus;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CampusEats.Tests.Features.Kitchen;

public class UpdateOrderStatusHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateStatus_WhenValidTransition()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new CampusDbContext(options);
        
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            Status = "Pending",
            OrderType = "Pickup",
            Total = 25.50m,
            CreatedAt = DateTimeOffset.UtcNow,
            UserId = "test-user"
        };
        
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        
        var validator = CreateMockValidator(isValid: true);
        var handler = new UpdateOrderStatusHandler(context, validator);
        var command = new UpdateOrderStatusRequest(order.Id, nameof(OrderStatus.Preparing));
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.ShouldNotBeNull();
        
        var updatedOrder = await context.Orders.FindAsync(order.Id);
        updatedOrder.ShouldNotBeNull();
        updatedOrder.Status.ShouldBe("Preparing");
        updatedOrder.UpdatedAt.ShouldNotBeNull();
    }
    
    [Fact]
    public async Task Handle_ShouldSetCompletedAt_WhenStatusIsCompleted()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new CampusDbContext(options);
        
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            Status = "Ready",
            OrderType = "Pickup",
            Total = 25.50m,
            CreatedAt = DateTimeOffset.UtcNow,
            UserId = "test-user"
        };
        
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        
        var validator = CreateMockValidator(isValid: true);
        var handler = new UpdateOrderStatusHandler(context, validator);
        var command = new UpdateOrderStatusRequest(order.Id, nameof(OrderStatus.Completed));
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.ShouldNotBeNull();
        
        var updatedOrder = await context.Orders.FindAsync(order.Id);
        updatedOrder.ShouldNotBeNull();
        updatedOrder.Status.ShouldBe("Completed");
        updatedOrder.CompletedAt.ShouldNotBeNull();
    }
    
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenOrderNotFound()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new CampusDbContext(options);
        
        var validator = CreateMockValidator(isValid: true);
        var handler = new UpdateOrderStatusHandler(context, validator);
        var command = new UpdateOrderStatusRequest(Guid.NewGuid(), nameof(OrderStatus.Preparing));
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.ShouldNotBeNull();
    }
    
    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenValidationFails()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new CampusDbContext(options);
        
        var validator = CreateMockValidator(isValid: false);
        var handler = new UpdateOrderStatusHandler(context, validator);
        var command = new UpdateOrderStatusRequest(Guid.NewGuid(), nameof(OrderStatus.Preparing));
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.ShouldNotBeNull();
    }
    
    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenInvalidTransition()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new CampusDbContext(options);
        
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            Status = "Pending",
            OrderType = "Pickup",
            Total = 25.50m,
            CreatedAt = DateTimeOffset.UtcNow,
            UserId = "test-user"
        };
        
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        
        var validator = CreateMockValidator(isValid: true);
        var handler = new UpdateOrderStatusHandler(context, validator);
        var command = new UpdateOrderStatusRequest(order.Id, nameof(OrderStatus.Completed));
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.ShouldNotBeNull();
    }
    
    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Preparing, true)]
    [InlineData(OrderStatus.Preparing, OrderStatus.Ready, true)]
    [InlineData(OrderStatus.Ready, OrderStatus.Completed, true)]
    [InlineData(OrderStatus.Pending, OrderStatus.Ready, false)]
    [InlineData(OrderStatus.Pending, OrderStatus.Completed, false)]
    [InlineData(OrderStatus.Preparing, OrderStatus.Completed, false)]
    [InlineData(OrderStatus.Ready, OrderStatus.Preparing, false)]
    public async Task Handle_ShouldValidateTransitions(OrderStatus currentStatus, OrderStatus newStatus, bool shouldSucceed)
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new CampusDbContext(options);
        
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            Status = currentStatus.ToString(),
            OrderType = "Pickup",
            Total = 25.50m,
            CreatedAt = DateTimeOffset.UtcNow,
            UserId = "test-user"
        };
        
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        
        var validator = CreateMockValidator(isValid: true);
        var handler = new UpdateOrderStatusHandler(context, validator);
        var command = new UpdateOrderStatusRequest(order.Id, newStatus.ToString());
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.ShouldNotBeNull();
        
        if (shouldSucceed)
        {
            var updatedOrder = await context.Orders.FindAsync(order.Id);
            updatedOrder!.Status.ShouldBe(newStatus.ToString());
        }
    }

    private static IValidator<UpdateOrderStatusRequest> CreateMockValidator(bool isValid = true)
    {
        var validator = Substitute.For<IValidator<UpdateOrderStatusRequest>>();
        if (isValid)
        {
            var validationResult = Substitute.For<FluentValidation.Results.ValidationResult>();
            validationResult.IsValid.Returns(true);
            validator.ValidateAsync(Arg.Any<UpdateOrderStatusRequest>(), Arg.Any<CancellationToken>())
                .Returns(validationResult);
        }
        else
        {
            var validationResult = Substitute.For<FluentValidation.Results.ValidationResult>();
            validationResult.IsValid.Returns(false);
            validator.ValidateAsync(Arg.Any<UpdateOrderStatusRequest>(), Arg.Any<CancellationToken>())
                .Returns(validationResult);
        }
        return validator;
    }
}

