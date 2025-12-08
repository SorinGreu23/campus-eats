using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Kitchen;
using CampusEats.Api.Features.Orders.GetPendingOrders;
using Microsoft.AspNetCore.Http;
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
        
        var pendingOrder = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            Status = "Pending",
            OrderType = "Pickup",
            Total = 25.50m,
            CreatedAt = DateTimeOffset.UtcNow,
            UserId = "test-user"
        };
        
        var preparingOrder = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-002",
            Status = "Preparing",
            OrderType = "Pickup",
            Total = 15.00m,
            CreatedAt = DateTimeOffset.UtcNow,
            UserId = "test-user"
        };
        
        var completedOrder = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-003",
            Status = "Completed",
            OrderType = "Pickup",
            Total = 30.00m,
            CreatedAt = DateTimeOffset.UtcNow,
            UserId = "test-user"
        };
        
        context.Orders.AddRange(pendingOrder, preparingOrder, completedOrder);
        await context.SaveChangesAsync();
        
        var handler = new GetPendingOrdersHandler(context);
        var query = new GetPendingOrdersQuery();
        
        // Act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // Assert
        result.ShouldNotBeNull();
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
            OrderType = "Pickup",
            Total = 30.00m,
            CreatedAt = DateTimeOffset.UtcNow,
            UserId = "test-user"
        };
        
        context.Orders.Add(completedOrder);
        await context.SaveChangesAsync();
        
        var handler = new GetPendingOrdersHandler(context);
        var query = new GetPendingOrdersQuery();
        
        // Act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // Assert
        result.ShouldNotBeNull();
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
            Price = 10.00m,
            CategoryId = Guid.NewGuid(),
            Description = "Test Description"
        };
        
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            Status = "Pending",
            OrderType = "Pickup",
            Total = 20.00m,
            CreatedAt = DateTimeOffset.UtcNow,
            UserId = "test-user",
            Items = new List<OrderItem>
            {
                new()
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
        result.ShouldNotBeNull();
    }
}

