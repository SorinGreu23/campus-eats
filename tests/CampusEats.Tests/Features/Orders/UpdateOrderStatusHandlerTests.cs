using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Kitchen;
using CampusEats.Api.Features.Orders.UpdateStatus;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Tests.Features.Orders;

public class UpdateOrderStatusHandlerTests
{
    private CampusDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new CampusDbContext(options);
    }

    [Fact]
    public async Task GivenNonExistentOrder_WhenUpdatingStatus_ThenReturnsNotFound()
    {
        // Arrange
        await using var context = CreateContext();
        var validator = new UpdateOrderStatusValidator();
        var handler = new UpdateOrderStatusHandler(context, validator);
        var request = new UpdateOrderStatusRequest(Guid.NewGuid(), "Preparing");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFound<string>>();
    }

    [Fact]
    public async Task GivenInvalidStatus_WhenUpdatingStatus_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-001",
            UserId = "user1",
            Status = "Pending",
            OrderType = "Pickup",
            Subtotal = 100m,
            Tax = 21m,
            Total = 121m,
            CreatedAt = DateTimeOffset.UtcNow
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var validator = new UpdateOrderStatusValidator();
        var handler = new UpdateOrderStatusHandler(context, validator);
        var request = new UpdateOrderStatusRequest(orderId, "InvalidStatus"
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().Should().Be(typeof(BadRequest<>));
    }

    [Fact]
    public async Task GivenValidTransitionFromPendingToPreparing_WhenUpdatingStatus_ThenSucceeds()
    {
        // Arrange
        await using var context = CreateContext();
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-001",
            UserId = "user1",
            Status = "Pending",
            OrderType = "Pickup",
            Subtotal = 100m,
            Tax = 21m,
            Total = 121m,
            CreatedAt = DateTimeOffset.UtcNow
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var validator = new UpdateOrderStatusValidator();
        var handler = new UpdateOrderStatusHandler(context, validator);
        var request = new UpdateOrderStatusRequest(orderId, "Preparing"
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContent>();
        
        var updatedOrder = await context.Orders.FindAsync(orderId);
        updatedOrder!.Status.Should().Be("Preparing");
    }

    [Fact]
    public async Task GivenValidTransitionFromPreparingToReady_WhenUpdatingStatus_ThenSucceeds()
    {
        // Arrange
        await using var context = CreateContext();
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-001",
            UserId = "user1",
            Status = "Preparing",
            OrderType = "Pickup",
            Subtotal = 100m,
            Tax = 21m,
            Total = 121m,
            CreatedAt = DateTimeOffset.UtcNow
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var validator = new UpdateOrderStatusValidator();
        var handler = new UpdateOrderStatusHandler(context, validator);
        var request = new UpdateOrderStatusRequest(orderId, "Ready"
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContent>();
        
        var updatedOrder = await context.Orders.FindAsync(orderId);
        updatedOrder!.Status.Should().Be("Ready");
    }

    [Fact]
    public async Task GivenValidTransitionFromReadyToCompleted_WhenUpdatingStatus_ThenSucceedsAndSetsCompletedAt()
    {
        // Arrange
        await using var context = CreateContext();
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-001",
            UserId = "user1",
            Status = "Ready",
            OrderType = "Pickup",
            Subtotal = 100m,
            Tax = 21m,
            Total = 121m,
            CreatedAt = DateTimeOffset.UtcNow
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var validator = new UpdateOrderStatusValidator();
        var handler = new UpdateOrderStatusHandler(context, validator);
        var request = new UpdateOrderStatusRequest(orderId, "Completed"
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContent>();
        
        var updatedOrder = await context.Orders.FindAsync(orderId);
        updatedOrder!.Status.Should().Be("Completed");
        updatedOrder.CompletedAt.Should().NotBeNull();
        updatedOrder.CompletedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GivenInvalidTransitionFromPendingToCompleted_WhenUpdatingStatus_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-001",
            UserId = "user1",
            Status = "Pending",
            OrderType = "Pickup",
            Subtotal = 100m,
            Tax = 21m,
            Total = 121m,
            CreatedAt = DateTimeOffset.UtcNow
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var validator = new UpdateOrderStatusValidator();
        var handler = new UpdateOrderStatusHandler(context, validator);
        var request = new UpdateOrderStatusRequest(orderId, "Completed"
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().Should().Be(typeof(BadRequest<>));
    }

    [Fact]
    public async Task GivenCancellationFromPending_WhenUpdatingStatus_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-001",
            UserId = "user1",
            Status = "Pending",
            OrderType = "Pickup",
            Subtotal = 100m,
            Tax = 21m,
            Total = 121m,
            CreatedAt = DateTimeOffset.UtcNow
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var validator = new UpdateOrderStatusValidator();
        var handler = new UpdateOrderStatusHandler(context, validator);
        var request = new UpdateOrderStatusRequest(orderId, "Cancelled"
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert - Cancelled is not allowed via UpdateStatus endpoint, use Cancel endpoint instead
        result.GetType().GetGenericTypeDefinition().Should().Be(typeof(BadRequest<>));
    }

    [Fact]
    public async Task GivenCancellationFromPreparing_WhenUpdatingStatus_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-001",
            UserId = "user1",
            Status = "Preparing",
            OrderType = "Pickup",
            Subtotal = 100m,
            Tax = 21m,
            Total = 121m,
            CreatedAt = DateTimeOffset.UtcNow
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var validator = new UpdateOrderStatusValidator();
        var handler = new UpdateOrderStatusHandler(context, validator);
        var request = new UpdateOrderStatusRequest(orderId, "Cancelled"
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert - Cancelled is not allowed via UpdateStatus endpoint, use Cancel endpoint instead
        result.GetType().GetGenericTypeDefinition().Should().Be(typeof(BadRequest<>));
    }

    [Fact]
    public async Task GivenCancellationFromCompleted_WhenUpdatingStatus_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-001",
            UserId = "user1",
            Status = "Completed",
            OrderType = "Pickup",
            Subtotal = 100m,
            Tax = 21m,
            Total = 121m,
            CreatedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var validator = new UpdateOrderStatusValidator();
        var handler = new UpdateOrderStatusHandler(context, validator);
        var request = new UpdateOrderStatusRequest(orderId, "Cancelled"
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().Should().Be(typeof(BadRequest<>));
    }

    [Fact]
    public async Task GivenPendingToPreparingTransition_WhenOrderHasInventoryItems_ThenDeductsInventory()
    {
        // Arrange
        await using var context = CreateContext();
        var orderId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();

        var inventoryItem = new InventoryItem
        {
            Id = inventoryItemId,
            Name = "Chicken Breast",
            Unit = "kg",
            CurrentQuantity = 10m,
            MinimumQuantity = 2m,
            IsOutOfStock = false,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Grilled Chicken",
            Description = "Delicious",
            Price = 15.00m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var ingredient = new MenuItemIngredient
        {
            MenuItemId = menuItemId,
            InventoryItemId = inventoryItemId,
            QuantityRequired = 0.25m
        };

        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-001",
            UserId = "user1",
            Status = "Pending",
            OrderType = "Pickup",
            Subtotal = 30m,
            Tax = 6.3m,
            Total = 36.3m,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var orderItem = new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            MenuItemId = menuItemId,
            Quantity = 2,
            UnitPrice = 15.00m,
            Subtotal = 30.00m
        };

        context.InventoryItems.Add(inventoryItem);
        context.MenuItems.Add(menuItem);
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        
        context.MenuItemIngredients.Add(ingredient);
        context.OrderItems.Add(orderItem);
        await context.SaveChangesAsync();

        var validator = new UpdateOrderStatusValidator();
        var handler = new UpdateOrderStatusHandler(context, validator);
        var request = new UpdateOrderStatusRequest(orderId, "Preparing"
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContent>();
        
        var updatedInventory = await context.InventoryItems.FindAsync(inventoryItemId);
        updatedInventory!.CurrentQuantity.Should().Be(9.5m); // 10 - (0.25 * 2)
    }

    [Fact]
    public async Task GivenPreparingToReadyTransition_WhenOrderHasInventoryItems_ThenDoesNotDeductInventory()
    {
        // Arrange
        await using var context = CreateContext();
        var orderId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();

        var inventoryItem = new InventoryItem
        {
            Id = inventoryItemId,
            Name = "Chicken Breast",
            Unit = "kg",
            CurrentQuantity = 10m,
            MinimumQuantity = 2m,
            IsOutOfStock = false,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Grilled Chicken",
            Description = "Delicious",
            Price = 15.00m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var ingredient = new MenuItemIngredient
        {
            MenuItemId = menuItemId,
            InventoryItemId = inventoryItemId,
            QuantityRequired = 0.25m
        };

        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-001",
            UserId = "user1",
            Status = "Preparing",
            OrderType = "Pickup",
            Subtotal = 30m,
            Tax = 6.3m,
            Total = 36.3m,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var orderItem = new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            MenuItemId = menuItemId,
            Quantity = 2,
            UnitPrice = 15.00m,
            Subtotal = 30.00m
        };

        context.InventoryItems.Add(inventoryItem);
        context.MenuItems.Add(menuItem);
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        
        context.MenuItemIngredients.Add(ingredient);
        context.OrderItems.Add(orderItem);
        await context.SaveChangesAsync();

        var validator = new UpdateOrderStatusValidator();
        var handler = new UpdateOrderStatusHandler(context, validator);
        var request = new UpdateOrderStatusRequest(orderId, "Ready"
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContent>();
        
        var updatedInventory = await context.InventoryItems.FindAsync(inventoryItemId);
        updatedInventory!.CurrentQuantity.Should().Be(10m); // No deduction
    }

    [Fact]
    public async Task GivenEmptyOrderId_WhenUpdatingStatus_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var validator = new UpdateOrderStatusValidator();
        var handler = new UpdateOrderStatusHandler(context, validator);
        var request = new UpdateOrderStatusRequest(Guid.Empty, "Preparing"
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().Should().Be(typeof(BadRequest<>));
    }

    [Fact]
    public async Task GivenNullStatus_WhenUpdatingStatus_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var validator = new UpdateOrderStatusValidator();
        var handler = new UpdateOrderStatusHandler(context, validator);
        var request = new UpdateOrderStatusRequest(Guid.NewGuid(), null!
        );

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().Should().Be(typeof(BadRequest<>));
    }
}

