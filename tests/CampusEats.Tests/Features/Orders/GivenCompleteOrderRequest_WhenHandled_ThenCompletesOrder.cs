using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Orders.Complete;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace CampusEats.Tests.Features.Orders;

public class CompleteOrderHandlerTests
{
    private CampusDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new CampusDbContext(options);
    }

    [Fact]
    public async Task GivenEmptyOrderId_WhenCompleting_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var handler = new CompleteOrderHandler(context);
        var request = new CompleteOrderRequest(Guid.Empty);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(BadRequest<>));
    }

    [Fact]
    public async Task GivenNonExistentOrder_WhenCompleting_ThenReturnsNotFound()
    {
        // Arrange
        await using var context = CreateContext();
        var handler = new CompleteOrderHandler(context);
        var request = new CompleteOrderRequest(Guid.NewGuid());

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(NotFound<>));
    }

    [Fact]
    public async Task GivenCancelledOrder_WhenCompleting_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-001",
            UserId = "user123",
            Status = "Cancelled",
            OrderType = "Pickup",
            CreatedAt = DateTimeOffset.UtcNow,
            Subtotal = 10.00m,
            Tax = 2.10m,
            Discount = 0m,
            Total = 12.10m,
            CancelledAt = DateTimeOffset.UtcNow
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var handler = new CompleteOrderHandler(context);
        var request = new CompleteOrderRequest(orderId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(BadRequest<>));
    }

    [Fact]
    public async Task GivenAlreadyCompletedOrder_WhenCompleting_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-002",
            UserId = "user123",
            Status = "Completed",
            OrderType = "Pickup",
            CreatedAt = DateTimeOffset.UtcNow,
            Subtotal = 10.00m,
            Tax = 2.10m,
            Discount = 0m,
            Total = 12.10m,
            CompletedAt = DateTimeOffset.UtcNow
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var handler = new CompleteOrderHandler(context);
        var request = new CompleteOrderRequest(orderId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(BadRequest<>));
    }

    [Fact]
    public async Task GivenPendingOrder_WhenCompleting_ThenCompletesSuccessfully()
    {
        // Arrange
        await using var context = CreateContext();
        var orderId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-003",
            UserId = "user123",
            Status = "Pending",
            OrderType = "Pickup",
            CreatedAt = DateTimeOffset.UtcNow,
            Subtotal = 15.00m,
            Tax = 3.15m,
            Discount = 0m,
            Total = 18.15m
        };
        var orderItem = new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            MenuItemId = menuItemId,
            Quantity = 2,
            UnitPrice = 7.50m,
            Subtotal = 15.00m
        };
        order.Items.Add(orderItem);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var handler = new CompleteOrderHandler(context);
        var request = new CompleteOrderRequest(orderId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(Ok<>));
        
        var updatedOrder = await context.Orders.FindAsync(orderId);
        updatedOrder.ShouldNotBeNull();
        updatedOrder.Status.ShouldBe("Completed");
        updatedOrder.CompletedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task GivenInProgressOrder_WhenCompleting_ThenCompletesSuccessfully()
    {
        // Arrange
        await using var context = CreateContext();
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-004",
            UserId = "user123",
            Status = "InProgress",
            OrderType = "Delivery",
            CreatedAt = DateTimeOffset.UtcNow,
            Subtotal = 20.00m,
            Tax = 4.20m,
            Discount = 2.00m,
            Total = 22.20m
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var handler = new CompleteOrderHandler(context);
        var request = new CompleteOrderRequest(orderId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(Ok<>));
        
        var updatedOrder = await context.Orders.FindAsync(orderId);
        updatedOrder.ShouldNotBeNull();
        updatedOrder.Status.ShouldBe("Completed");
        updatedOrder.CompletedAt.ShouldNotBeNull();
        updatedOrder.CompletedAt.Value.ShouldBeGreaterThan(DateTimeOffset.UtcNow.AddMinutes(-1));
    }
}
