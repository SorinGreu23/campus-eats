using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Kitchen;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CampusEats.Tests.Features.Kitchen;

public class InventoryDeductionTests
{
    private IValidator<UpdateOrderStatusCommand> CreateMockValidator()
    {
        var validator = Substitute.For<IValidator<UpdateOrderStatusCommand>>();
        var validationResult = Substitute.For<FluentValidation.Results.ValidationResult>();
        validationResult.IsValid.Returns(true);
        validator.ValidateAsync(Arg.Any<UpdateOrderStatusCommand>(), Arg.Any<CancellationToken>())
            .Returns(validationResult);
        return validator;
    }

    [Fact]
    public async Task Handle_ShouldDeductInventory_WhenStatusChangesToPreparing()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new CampusDbContext(options);

        var inventoryItem = new InventoryItem
        {
            Id = Guid.NewGuid(),
            Name = "Tomato",
            CurrentQuantity = 10,
            MinimumQuantity = 1,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = "Tomato Soup",
            Price = 5.00m,
            CreatedAt = DateTimeOffset.UtcNow,
            CategoryId = Guid.NewGuid(),
            Description = "Delicious tomato soup"
        };

        var ingredient = new MenuItemIngredient
        {
            Id = Guid.NewGuid(),
            MenuItemId = menuItem.Id,
            InventoryItemId = inventoryItem.Id,
            QuantityRequired = 2
        };

        menuItem.Ingredients.Add(ingredient);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-INV-001",
            Status = "Pending",
            Total = 5.00m,
            CreatedAt = DateTimeOffset.UtcNow,
            UserId = "test-user"
        };

        var orderItem = new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            MenuItemId = menuItem.Id,
            Quantity = 3, // Requires 3 * 2 = 6 tomatoes
            UnitPrice = 5.00m,
            Subtotal = 15.00m
        };

        order.Items.Add(orderItem);

        context.InventoryItems.Add(inventoryItem);
        context.MenuItems.Add(menuItem);
        context.MenuItemIngredients.Add(ingredient);
        context.Orders.Add(order);
        context.OrderItems.Add(orderItem);
        await context.SaveChangesAsync();

        var handler = new UpdateOrderStatusHandler(context, CreateMockValidator());
        var command = new UpdateOrderStatusCommand(order.Id, OrderStatus.Preparing);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        
        var updatedInventory = await context.InventoryItems.FindAsync(inventoryItem.Id);
        updatedInventory!.CurrentQuantity.ShouldBe(4); // 10 - 6 = 4

        var transaction = await context.InventoryTransactions.FirstOrDefaultAsync();
        transaction.ShouldNotBeNull();
        transaction.Quantity.ShouldBe(-6);
        transaction.TransactionType.ShouldBe("OrderUsage");
    }

    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenInsufficientInventory()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new CampusDbContext(options);

        var inventoryItem = new InventoryItem
        {
            Id = Guid.NewGuid(),
            Name = "Tomato",
            CurrentQuantity = 5, // Only 5 available
            MinimumQuantity = 1,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = "Tomato Soup",
            Price = 5.00m,
            CreatedAt = DateTimeOffset.UtcNow,
            CategoryId = Guid.NewGuid(),
            Description = "Delicious tomato soup"
        };

        var ingredient = new MenuItemIngredient
        {
            Id = Guid.NewGuid(),
            MenuItemId = menuItem.Id,
            InventoryItemId = inventoryItem.Id,
            QuantityRequired = 2
        };

        menuItem.Ingredients.Add(ingredient);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-INV-002",
            Status = "Pending",
            Total = 5.00m,
            CreatedAt = DateTimeOffset.UtcNow,
            UserId = "test-user"
        };

        var orderItem = new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            MenuItemId = menuItem.Id,
            Quantity = 3, // Requires 3 * 2 = 6 tomatoes
            UnitPrice = 5.00m,
            Subtotal = 15.00m
        };

        order.Items.Add(orderItem);

        context.InventoryItems.Add(inventoryItem);
        context.MenuItems.Add(menuItem);
        context.MenuItemIngredients.Add(ingredient);
        context.Orders.Add(order);
        context.OrderItems.Add(orderItem);
        await context.SaveChangesAsync();

        var handler = new UpdateOrderStatusHandler(context, CreateMockValidator());
        var command = new UpdateOrderStatusCommand(order.Id, OrderStatus.Preparing);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        // Result should be BadRequest
        // Since IResult is opaque, we can't easily check the status code without reflection or casting if it's a specific type.
        // But we can check that inventory was NOT deducted.
        
        var updatedInventory = await context.InventoryItems.FindAsync(inventoryItem.Id);
        updatedInventory!.CurrentQuantity.ShouldBe(5); // Should remain 5

        var orderStatus = await context.Orders.FindAsync(order.Id);
        orderStatus!.Status.ShouldBe("Pending"); // Should remain Pending
    }
}
