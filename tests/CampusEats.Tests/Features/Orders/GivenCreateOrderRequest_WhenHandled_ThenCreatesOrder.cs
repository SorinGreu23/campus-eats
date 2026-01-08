using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Orders.Create;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using System.Security.Claims;

namespace CampusEats.Tests.Features.Orders;

public class CreateOrderHandlerTests
{
    private CampusDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new CampusDbContext(options);
    }

    private IHttpContextAccessor CreateMockHttpContextAccessor(string userId, bool isAuthenticated = true)
    {
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            isAuthenticated ? new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            } : Array.Empty<Claim>(),
            isAuthenticated ? "TestAuth" : null
        ));

        httpContext.User.Returns(user);
        httpContextAccessor.HttpContext.Returns(httpContext);

        return httpContextAccessor;
    }

    [Fact]
    public async Task GivenUnauthenticatedUser_WhenCreatingOrder_ThenReturnsUnauthorized()
    {
        // Arrange
        await using var context = CreateContext();
        var httpContextAccessor = CreateMockHttpContextAccessor("user123", isAuthenticated: false);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = "user123",
            Items = new List<CreateOrderItemRequest>
            {
                new() { MenuItemId = Guid.NewGuid(), Quantity = 1 }
            }
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<UnauthorizedHttpResult>();
    }

    [Fact]
    public async Task GivenEmptyItems_WhenCreatingOrder_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            Items = new List<CreateOrderItemRequest>()
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequest<object>>();
    }

    [Fact]
    public async Task GivenNullItems_WhenCreatingOrder_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            Items = null
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequest<object>>();
    }

    [Fact]
    public async Task GivenMismatchedUserId_WhenCreatingOrder_ThenReturnsForbid()
    {
        // Arrange
        await using var context = CreateContext();
        var authenticatedUserId = "user123";
        var requestUserId = "user456";
        var httpContextAccessor = CreateMockHttpContextAccessor(authenticatedUserId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = requestUserId,
            Items = new List<CreateOrderItemRequest>
            {
                new() { MenuItemId = Guid.NewGuid(), Quantity = 1 }
            }
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<ForbidHttpResult>();
    }

    [Fact]
    public async Task GivenNonExistentMenuItem_WhenCreatingOrder_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            Items = new List<CreateOrderItemRequest>
            {
                new() { MenuItemId = Guid.NewGuid(), Quantity = 1 }
            }
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequest<object>>();
    }

    [Fact]
    public async Task GivenValidOrder_WhenCreatingOrder_ThenCreatesSuccessfully()
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
        context.MenuItems.Add(menuItem);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            OrderType = "Pickup",
            DeliveryInstructions = "No onions",
            Items = new List<CreateOrderItemRequest>
            {
                new() { MenuItemId = menuItemId, Quantity = 2, SpecialInstructions = "Extra cheese" }
            }
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Created<object>>();
        
        var createdOrder = await context.Orders.Include(o => o.Items).FirstOrDefaultAsync();
        createdOrder.ShouldNotBeNull();
        createdOrder.UserId.ShouldBe(userId);
        createdOrder.Status.ShouldBe("Pending");
        createdOrder.Subtotal.ShouldBe(20.00m);
        createdOrder.Tax.ShouldBe(4.20m); // 21% tax
        createdOrder.Total.ShouldBe(24.20m);
        createdOrder.Items.Count.ShouldBe(1);
        createdOrder.Items.First().Quantity.ShouldBe(2);
        createdOrder.Items.First().UnitPrice.ShouldBe(10.00m);
        createdOrder.Items.First().Subtotal.ShouldBe(20.00m);
    }

    [Fact]
    public async Task GivenMultipleItems_WhenCreatingOrder_ThenCalculatesTotalCorrectly()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var menuItem1Id = Guid.NewGuid();
        var menuItem2Id = Guid.NewGuid();
        
        var menuItem1 = new MenuItem
        {
            Id = menuItem1Id,
            Name = "Burger",
            Description = "Tasty burger",
            Price = 10.00m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        var menuItem2 = new MenuItem
        {
            Id = menuItem2Id,
            Name = "Fries",
            Description = "Crispy fries",
            Price = 5.00m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        context.MenuItems.AddRange(menuItem1, menuItem2);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            Items = new List<CreateOrderItemRequest>
            {
                new() { MenuItemId = menuItem1Id, Quantity = 2 },
                new() { MenuItemId = menuItem2Id, Quantity = 3 }
            }
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Created<object>>();
        
        var createdOrder = await context.Orders.Include(o => o.Items).FirstOrDefaultAsync();
        createdOrder.ShouldNotBeNull();
        createdOrder.Subtotal.ShouldBe(35.00m); // 2*10 + 3*5
        createdOrder.Tax.ShouldBe(7.35m); // 35 * 0.21
        createdOrder.Total.ShouldBe(42.35m);
        createdOrder.Items.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GivenDeliveryOrderType_WhenCreatingOrder_ThenPickupTimeIsNull()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var menuItemId = Guid.NewGuid();
        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Pizza",
            Description = "Cheesy pizza",
            Price = 12.00m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        context.MenuItems.Add(menuItem);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            OrderType = "Delivery",
            Items = new List<CreateOrderItemRequest>
            {
                new() { MenuItemId = menuItemId, Quantity = 1 }
            }
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Created<object>>();
        
        var createdOrder = await context.Orders.FirstOrDefaultAsync();
        createdOrder.ShouldNotBeNull();
        createdOrder.OrderType.ShouldBe("Delivery");
        createdOrder.PickupTime.ShouldBeNull();
    }
}
