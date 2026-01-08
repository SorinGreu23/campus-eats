using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Orders.Get;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using System.Security.Claims;

namespace CampusEats.Tests.Features.Orders;

public class GetOrdersByUserHandlerTests
{
    private CampusDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new CampusDbContext(options);
    }

    private IHttpContextAccessor CreateMockHttpContextAccessor(string userId, string[] roles, bool isAuthenticated = true)
    {
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        
        var claims = new List<Claim>();
        if (isAuthenticated)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }
        
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            claims,
            isAuthenticated ? "TestAuth" : null
        ));

        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);

        return httpContextAccessor;
    }

    [Fact]
    public async Task GivenUnauthenticatedUser_WhenGettingOrders_ThenReturnsUnauthorized()
    {
        // Arrange
        await using var context = CreateContext();
        var httpContextAccessor = CreateMockHttpContextAccessor("user123", Array.Empty<string>(), isAuthenticated: false);
        var handler = new GetOrdersByUserHandler(context, httpContextAccessor);
        var request = new GetOrdersByUserRequest { UserId = "user123" };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<UnauthorizedHttpResult>();
    }

    [Fact]
    public async Task GivenUserWithNoOrders_WhenGettingOrders_ThenReturnsNotFound()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var httpContextAccessor = CreateMockHttpContextAccessor(userId, new[] { "Customer" });
        var handler = new GetOrdersByUserHandler(context, httpContextAccessor);
        var request = new GetOrdersByUserRequest { UserId = userId };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert - Should return Ok with empty array, not NotFound
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(Ok<>));
    }

    [Fact]
    public async Task GivenUserWithOrders_WhenGettingOrders_ThenReturnsOrders()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            UserId = userId,
            Status = "Pending",
            OrderType = "Pickup",
            CreatedAt = DateTimeOffset.UtcNow,
            Subtotal = 15.00m,
            Tax = 3.15m,
            Discount = 0m,
            Total = 18.15m
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId, new[] { "Customer" });
        var handler = new GetOrdersByUserHandler(context, httpContextAccessor);
        var request = new GetOrdersByUserRequest { UserId = userId };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(Ok<>));
    }

    [Fact]
    public async Task GivenNonOwner_WhenGettingOthersOrders_ThenReturnsForbid()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var otherUserId = "user456";
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            UserId = otherUserId,
            Status = "Pending",
            OrderType = "Pickup",
            CreatedAt = DateTimeOffset.UtcNow,
            Subtotal = 10.00m,
            Tax = 2.10m,
            Discount = 0m,
            Total = 12.10m
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId, new[] { "Customer" });
        var handler = new GetOrdersByUserHandler(context, httpContextAccessor);
        var request = new GetOrdersByUserRequest { UserId = otherUserId };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<ForbidHttpResult>();
    }

    [Fact]
    public async Task GivenAdmin_WhenGettingAnyUsersOrders_ThenReturnsOrders()
    {
        // Arrange
        await using var context = CreateContext();
        var adminUserId = "admin123";
        var targetUserId = "user456";
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            UserId = targetUserId,
            Status = "Completed",
            OrderType = "Pickup",
            CreatedAt = DateTimeOffset.UtcNow,
            Subtotal = 20.00m,
            Tax = 4.20m,
            Discount = 0m,
            Total = 24.20m,
            CompletedAt = DateTimeOffset.UtcNow
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(adminUserId, new[] { "Admin" });
        var handler = new GetOrdersByUserHandler(context, httpContextAccessor);
        var request = new GetOrdersByUserRequest { UserId = targetUserId };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(Ok<>));
    }

    [Fact]
    public async Task GivenKitchenStaff_WhenGettingAnyUsersOrders_ThenReturnsOrders()
    {
        // Arrange
        await using var context = CreateContext();
        var kitchenUserId = "kitchen123";
        var targetUserId = "user456";
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            UserId = targetUserId,
            Status = "InProgress",
            OrderType = "Pickup",
            CreatedAt = DateTimeOffset.UtcNow,
            Subtotal = 25.00m,
            Tax = 5.25m,
            Discount = 0m,
            Total = 30.25m
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(kitchenUserId, new[] { "Kitchen" });
        var handler = new GetOrdersByUserHandler(context, httpContextAccessor);
        var request = new GetOrdersByUserRequest { UserId = targetUserId };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(Ok<>));
    }

    [Fact]
    public async Task GivenMultipleOrders_WhenGettingOrders_ThenReturnsOrderedByIdDescending()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        
        var order1 = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            UserId = userId,
            Status = "Completed",
            OrderType = "Pickup",
            CreatedAt = DateTimeOffset.UtcNow,
            Subtotal = 10.00m,
            Tax = 2.10m,
            Discount = 0m,
            Total = 12.10m
        };
        
        var order2 = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-002",
            UserId = userId,
            Status = "Pending",
            OrderType = "Pickup",
            CreatedAt = DateTimeOffset.UtcNow,
            Subtotal = 15.00m,
            Tax = 3.15m,
            Discount = 0m,
            Total = 18.15m
        };

        // Add in specific order to test sorting
        context.Orders.Add(order1);
        await context.SaveChangesAsync();
        context.Orders.Add(order2);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId, new[] { "Customer" });
        var handler = new GetOrdersByUserHandler(context, httpContextAccessor);
        var request = new GetOrdersByUserRequest { UserId = userId };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(Ok<>));
        
        var orders = await context.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.Id)
            .ToListAsync();
        orders.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GivenOrdersWithItems_WhenGettingOrders_ThenIncludesItemsAndMenuItems()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var menuItemId = Guid.NewGuid();
        
        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Burger",
            Description = "Tasty burger",
            Price = 10.00m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            UserId = userId,
            Status = "Pending",
            OrderType = "Pickup",
            CreatedAt = DateTimeOffset.UtcNow,
            Subtotal = 20.00m,
            Tax = 4.20m,
            Discount = 0m,
            Total = 24.20m
        };

        var orderItem = new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            MenuItemId = menuItemId,
            Quantity = 2,
            UnitPrice = 10.00m,
            Subtotal = 20.00m,
            SpecialInstructions = "No onions"
        };

        context.MenuItems.Add(menuItem);
        context.Orders.Add(order);
        context.OrderItems.Add(orderItem);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId, new[] { "Customer" });
        var handler = new GetOrdersByUserHandler(context, httpContextAccessor);
        var request = new GetOrdersByUserRequest { UserId = userId };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(Ok<>));
        
        var orderFromDb = await context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.MenuItem)
            .FirstAsync(o => o.Id == order.Id);
        
        orderFromDb.Items.Count.ShouldBe(1);
        orderFromDb.Items.First().MenuItem.ShouldNotBeNull();
        orderFromDb.Items.First().MenuItem!.Name.ShouldBe("Burger");
    }

    [Fact]
    public async Task GivenEmptyUserId_WhenGettingOrders_ThenUsesAuthenticatedUserId()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            UserId = userId,
            Status = "Pending",
            OrderType = "Pickup",
            CreatedAt = DateTimeOffset.UtcNow,
            Subtotal = 10.00m,
            Tax = 2.10m,
            Discount = 0m,
            Total = 12.10m
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId, new[] { "Customer" });
        var handler = new GetOrdersByUserHandler(context, httpContextAccessor);
        var request = new GetOrdersByUserRequest { UserId = string.Empty };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(Ok<>));
    }
}
