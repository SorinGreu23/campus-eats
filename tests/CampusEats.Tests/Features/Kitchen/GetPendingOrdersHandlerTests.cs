using CampusEats.Api.Common.Models;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Kitchen;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace CampusEats.Tests.Features.Kitchen;

public class GetPendingOrdersHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnPendingOrders_WhenTheyExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new CampusDbContext(options);
        
        // Create test data
        var pendingOrder = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            Status = "Pending",
            Total = 25.50m,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        var preparingOrder = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-002",
            Status = "Preparing",
            Total = 15.00m,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        var completedOrder = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-003",
            Status = "Completed",
            Total = 30.00m,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        context.Orders.AddRange(pendingOrder, preparingOrder, completedOrder);
        await context.SaveChangesAsync();
        
        var handler = new GetPendingOrdersHandler(context);
        var query = new GetPendingOrdersQuery();
        
        // Act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(2);
        result.Value.ShouldContain(o => o.Status == "Pending");
        result.Value.ShouldContain(o => o.Status == "Preparing");
        result.Value.ShouldNotContain(o => o.Status == "Completed");
    }
    
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoPendingOrders()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new CampusDbContext(options);
        
        var completedOrder = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            Status = "Completed",
            Total = 30.00m,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        context.Orders.Add(completedOrder);
        await context.SaveChangesAsync();
        
        var handler = new GetPendingOrdersHandler(context);
        var query = new GetPendingOrdersQuery();
        
        // Act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(0);
    }
    
    [Fact]
    public async Task Handle_ShouldIncludeOrderItems_WhenTheyExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new CampusDbContext(options);
        
        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = "Pizza",
            Price = 10.00m
        };
        
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            Status = "Pending",
            Total = 20.00m,
            CreatedAt = DateTimeOffset.UtcNow,
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    MenuItemId = menuItem.Id,
                    Quantity = 2,
                    UnitPrice = 10.00m,
                    Subtotal = 20.00m
                }
            }
        };
        
        context.MenuItems.Add(menuItem);
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        
        var handler = new GetPendingOrdersHandler(context);
        var query = new GetPendingOrdersQuery();
        
        // Act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(1);
        result.Value[0].Items.Count.ShouldBe(1);
        result.Value[0].Items[0].MenuItemName.ShouldBe("Pizza");
        result.Value[0].Items[0].Quantity.ShouldBe(2);
    }
}

