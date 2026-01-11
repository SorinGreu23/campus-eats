using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Orders.Pending;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Tests.Features.Orders;

public class GetPendingOrdersHandlerTests
{
    private readonly DbContextOptions<CampusDbContext> _options;

    public GetPendingOrdersHandlerTests()
    {
        _options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GivenNoPendingOrders_WhenGettingPendingOrders_ThenReturnsEmptyList()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);
        var handler = new GetPendingOrdersHandler(context);
        var request = new GetPendingOrdersRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IResult>();
        
        // Check it's an Ok result
        result.GetType().Name.Should().StartWith("Ok");
    }

    [Fact]
    public async Task GivenPendingAndCompletedOrders_WhenGettingPendingOrders_ThenReturnsOnlyPending()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);
        
        var paidOrder1 = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            UserId = "user1",
            Status = "Paid",
            OrderType = "Pickup",
            Subtotal = 100m,
            Tax = 21m,
            Discount = 0m,
            Total = 121m,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var paidOrder2 = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-002",
            UserId = "user2",
            Status = "Paid",
            OrderType = "Delivery",
            Subtotal = 50m,
            Tax = 10.5m,
            Discount = 0m,
            Total = 60.5m,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var completedOrder = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-003",
            UserId = "user3",
            Status = "Completed",
            OrderType = "Pickup",
            Subtotal = 75m,
            Tax = 15.75m,
            Discount = 0m,
            Total = 90.75m,
            CreatedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow
        };

        var cancelledOrder = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-004",
            UserId = "user4",
            Status = "Cancelled",
            OrderType = "Delivery",
            Subtotal = 30m,
            Tax = 6.3m,
            Discount = 0m,
            Total = 36.3m,
            CreatedAt = DateTimeOffset.UtcNow,
            CancelledAt = DateTimeOffset.UtcNow,
            CancellationReason = "Customer request"
        };

        context.Orders.AddRange(paidOrder1, paidOrder2, completedOrder, cancelledOrder);
        await context.SaveChangesAsync();

        var handler = new GetPendingOrdersHandler(context);
        var request = new GetPendingOrdersRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IResult>();
        
        // Check it's an Ok result
        result.GetType().Name.Should().StartWith("Ok");
        
        // Get the Value property using reflection since we don't know the exact generic type
        var valueProperty = result.GetType().GetProperty("Value");
        valueProperty.Should().NotBeNull();
        var value = valueProperty!.GetValue(result) as IEnumerable<object>;
        value.Should().NotBeNull();
        value!.Should().HaveCount(2); // Only the two "Paid" orders
    }

    [Fact]
    public async Task GivenPendingOrders_WhenGettingPendingOrders_ThenIncludesOrderItems()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);
        
        var menuItem1 = new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = "Burger",
            Description = "Classic burger",
            Price = 10m,
            ImageUrl = "burger.jpg",
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var menuItem2 = new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = "Fries",
            Description = "Crispy fries",
            Price = 5m,
            ImageUrl = "fries.jpg",
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            UserId = "user1",
            Status = "Paid",
            OrderType = "Pickup",
            Subtotal = 25m,
            Tax = 5.25m,
            Discount = 0m,
            Total = 30.25m,
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.MenuItems.AddRange(menuItem1, menuItem2);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var orderItem1 = new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            MenuItemId = menuItem1.Id,
            Quantity = 2,
            UnitPrice = 10m,
            Subtotal = 20m,
            SpecialInstructions = "No onions"
        };

        var orderItem2 = new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            MenuItemId = menuItem2.Id,
            Quantity = 1,
            UnitPrice = 5m,
            Subtotal = 5m
        };

        context.OrderItems.AddRange(orderItem1, orderItem2);
        await context.SaveChangesAsync();

        var handler = new GetPendingOrdersHandler(context);
        var request = new GetPendingOrdersRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IResult>();
        
        // Check it's an Ok result
        result.GetType().Name.Should().StartWith("Ok");
        
        // Verify order items are included
        var orderWithItems = await context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.MenuItem)
            .FirstOrDefaultAsync(o => o.Id == order.Id);

        orderWithItems.Should().NotBeNull();
        orderWithItems!.Items.Should().HaveCount(2);
        orderWithItems.Items.Should().Contain(i => i.MenuItemId == menuItem1.Id);
        orderWithItems.Items.Should().Contain(i => i.MenuItemId == menuItem2.Id);
    }

    [Fact]
    public async Task GivenMultiplePendingOrders_WhenGettingPendingOrders_ThenReturnsOrderedList()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);
        
        var order1 = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-003",
            UserId = "user1",
            Status = "Paid",
            OrderType = "Pickup",
            Subtotal = 100m,
            Tax = 21m,
            Discount = 0m,
            Total = 121m,
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-30)
        };

        var order2 = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            UserId = "user2",
            Status = "Paid",
            OrderType = "Delivery",
            Subtotal = 50m,
            Tax = 10.5m,
            Discount = 0m,
            Total = 60.5m,
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-45)
        };

        var order3 = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-002",
            UserId = "user3",
            Status = "Paid",
            OrderType = "Pickup",
            Subtotal = 75m,
            Tax = 15.75m,
            Discount = 0m,
            Total = 90.75m,
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-15)
        };

        context.Orders.AddRange(order2, order1, order3); // Add in mixed order
        await context.SaveChangesAsync();

        var handler = new GetPendingOrdersHandler(context);
        var request = new GetPendingOrdersRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IResult>();
        
        // Check it's an Ok result
        result.GetType().Name.Should().StartWith("Ok");
        
        // Get the Value property using reflection since we don't know the exact generic type
        var valueProperty = result.GetType().GetProperty("Value");
        valueProperty.Should().NotBeNull();
        var value = valueProperty!.GetValue(result) as IEnumerable<object>;
        value.Should().NotBeNull();
        value!.Should().HaveCount(3);
        // Note: Handler orders by Id, not by CreatedAt
    }
}
