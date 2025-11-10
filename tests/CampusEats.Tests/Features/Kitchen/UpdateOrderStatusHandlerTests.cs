using CampusEats.Api.Common.Models;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Kitchen;
using Microsoft.EntityFrameworkCore;
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
            Total = 25.50m,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        
        var handler = new UpdateOrderStatusHandler(context);
        var command = new UpdateOrderStatusCommand(order.Id, "Preparing");
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        
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
            Total = 25.50m,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        
        var handler = new UpdateOrderStatusHandler(context);
        var command = new UpdateOrderStatusCommand(order.Id, "Completed");
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        
        var updatedOrder = await context.Orders.FindAsync(order.Id);
        updatedOrder.ShouldNotBeNull();
        updatedOrder.Status.ShouldBe("Completed");
        updatedOrder.CompletedAt.ShouldNotBeNull();
    }
    
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenOrderNotFound()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new CampusDbContext(options);
        
        var handler = new UpdateOrderStatusHandler(context);
        var command = new UpdateOrderStatusCommand(Guid.NewGuid(), "Preparing");
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.ShouldContain("Order not found");
    }
    
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenInvalidTransition()
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
            Total = 25.50m,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        
        var handler = new UpdateOrderStatusHandler(context);
        var command = new UpdateOrderStatusCommand(order.Id, "Completed"); // Invalid: Pending -> Completed
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.ShouldContain("Invalid status transition");
    }
    
    [Theory]
    [InlineData("Pending", "Preparing", true)]
    [InlineData("Preparing", "Ready", true)]
    [InlineData("Ready", "Completed", true)]
    [InlineData("Pending", "Ready", false)]
    [InlineData("Pending", "Completed", false)]
    [InlineData("Preparing", "Completed", false)]
    [InlineData("Ready", "Preparing", false)]
    public async Task Handle_ShouldValidateTransitions(string currentStatus, string newStatus, bool shouldSucceed)
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
            Status = currentStatus,
            Total = 25.50m,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        
        var handler = new UpdateOrderStatusHandler(context);
        var command = new UpdateOrderStatusCommand(order.Id, newStatus);
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.ShouldBe(shouldSucceed);
        if (shouldSucceed)
        {
            var updatedOrder = await context.Orders.FindAsync(order.Id);
            updatedOrder!.Status.ShouldBe(newStatus);
        }
        else
        {
            result.Error.ShouldNotBeNull();
            result.Error.ShouldContain("Invalid status transition");
        }
    }
}

