using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Orders.Cancel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using System.Security.Claims;

namespace CampusEats.Tests.Features.Orders;

public class CancelOrderHandlerTests
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
    public async Task GivenUnauthenticatedUser_WhenCancellingOrder_ThenReturnsUnauthorized()
    {
        // Arrange
        await using var context = CreateContext();
        var httpContextAccessor = CreateMockHttpContextAccessor("user123", Array.Empty<string>(), isAuthenticated: false);
        var handler = new CancelOrderHandler(context, httpContextAccessor);
        var request = new CancelOrderRequest
        {
            OrderId = Guid.NewGuid(),
            Reason = "Changed my mind"
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<UnauthorizedHttpResult>();
    }

    [Fact]
    public async Task GivenEmptyOrderId_WhenCancellingOrder_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var httpContextAccessor = CreateMockHttpContextAccessor("user123", new[] { "Customer" });
        var handler = new CancelOrderHandler(context, httpContextAccessor);
        var request = new CancelOrderRequest
        {
            OrderId = Guid.Empty,
            Reason = "Test"
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(BadRequest<>));
    }

    [Fact]
    public async Task GivenNonExistentOrder_WhenCancellingOrder_ThenReturnsNotFound()
    {
        // Arrange
        await using var context = CreateContext();
        var httpContextAccessor = CreateMockHttpContextAccessor("user123", new[] { "Customer" });
        var handler = new CancelOrderHandler(context, httpContextAccessor);
        var request = new CancelOrderRequest
        {
            OrderId = Guid.NewGuid(),
            Reason = "Test"
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(NotFound<>));
    }

    [Fact]
    public async Task GivenAlreadyCancelledOrder_WhenCancellingOrder_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-001",
            UserId = userId,
            Status = "Cancelled",
            OrderType = "Pickup",
            CreatedAt = DateTimeOffset.UtcNow,
            Subtotal = 10.00m,
            Tax = 2.10m,
            Discount = 0m,
            Total = 12.10m,
            CancelledAt = DateTimeOffset.UtcNow,
            CancellationReason = "Already cancelled"
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId, new[] { "Customer" });
        var handler = new CancelOrderHandler(context, httpContextAccessor);
        var request = new CancelOrderRequest
        {
            OrderId = orderId,
            Reason = "Trying to cancel again"
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(BadRequest<>));
    }

    [Fact]
    public async Task GivenCompletedOrder_WhenCancellingOrder_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-002",
            UserId = userId,
            Status = "Completed",
            OrderType = "Pickup",
            CreatedAt = DateTimeOffset.UtcNow,
            Subtotal = 15.00m,
            Tax = 3.15m,
            Discount = 0m,
            Total = 18.15m,
            CompletedAt = DateTimeOffset.UtcNow
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId, new[] { "Customer" });
        var handler = new CancelOrderHandler(context, httpContextAccessor);
        var request = new CancelOrderRequest
        {
            OrderId = orderId,
            Reason = "Want to cancel"
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(BadRequest<>));
    }

    [Fact]
    public async Task GivenAdminUser_WhenCancellingAnyOrder_ThenCancelsSuccessfully()
    {
        // Arrange
        await using var context = CreateContext();
        var adminUserId = "admin123";
        var orderOwnerId = "customer456";
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-003",
            UserId = orderOwnerId,
            Status = "Pending",
            OrderType = "Pickup",
            CreatedAt = DateTimeOffset.UtcNow,
            Subtotal = 20.00m,
            Tax = 4.20m,
            Discount = 0m,
            Total = 24.20m
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(adminUserId, new[] { "Admin" });
        var handler = new CancelOrderHandler(context, httpContextAccessor);
        var request = new CancelOrderRequest
        {
            OrderId = orderId,
            Reason = "Admin cancellation"
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(Ok<>));
        
        var cancelledOrder = await context.Orders.FindAsync(orderId);
        cancelledOrder.ShouldNotBeNull();
        cancelledOrder.Status.ShouldBe("Cancelled");
        cancelledOrder.CancelledAt.ShouldNotBeNull();
        cancelledOrder.CancellationReason.ShouldBe("Admin cancellation");
    }

    [Fact]
    public async Task GivenKitchenUser_WhenCancellingOrder_ThenCancelsSuccessfully()
    {
        // Arrange
        await using var context = CreateContext();
        var kitchenUserId = "kitchen123";
        var orderOwnerId = "customer456";
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-004",
            UserId = orderOwnerId,
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
        var handler = new CancelOrderHandler(context, httpContextAccessor);
        var request = new CancelOrderRequest
        {
            OrderId = orderId,
            Reason = "Kitchen issue"
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(Ok<>));
        
        var cancelledOrder = await context.Orders.FindAsync(orderId);
        cancelledOrder.ShouldNotBeNull();
        cancelledOrder.Status.ShouldBe("Cancelled");
        cancelledOrder.CancellationReason.ShouldBe("Kitchen issue");
    }

    [Fact]
    public async Task GivenOrderOwner_WhenCancellingOwnOrder_ThenCancelsSuccessfully()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-005",
            UserId = userId,
            Status = "Pending",
            OrderType = "Pickup",
            CreatedAt = DateTimeOffset.UtcNow,
            Subtotal = 12.00m,
            Tax = 2.52m,
            Discount = 0m,
            Total = 14.52m
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId, new[] { "Customer" });
        var handler = new CancelOrderHandler(context, httpContextAccessor);
        var request = new CancelOrderRequest
        {
            OrderId = orderId,
            Reason = "Changed my mind"
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(Ok<>));
        
        var cancelledOrder = await context.Orders.FindAsync(orderId);
        cancelledOrder.ShouldNotBeNull();
        cancelledOrder.Status.ShouldBe("Cancelled");
        cancelledOrder.CancelledAt.ShouldNotBeNull();
        cancelledOrder.CancellationReason.ShouldBe("Changed my mind");
    }

    [Fact]
    public async Task GivenNonOwner_WhenCancellingOthersOrder_ThenReturnsForbid()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var otherUserId = "user456";
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-006",
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
        var handler = new CancelOrderHandler(context, httpContextAccessor);
        var request = new CancelOrderRequest
        {
            OrderId = orderId,
            Reason = "Trying to cancel"
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<ForbidHttpResult>();
    }

    [Fact]
    public async Task GivenCancellationWithoutReason_WhenCancelling_ThenCancelsWithNullReason()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-007",
            UserId = userId,
            Status = "Pending",
            OrderType = "Pickup",
            CreatedAt = DateTimeOffset.UtcNow,
            Subtotal = 8.00m,
            Tax = 1.68m,
            Discount = 0m,
            Total = 9.68m
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId, new[] { "Customer" });
        var handler = new CancelOrderHandler(context, httpContextAccessor);
        var request = new CancelOrderRequest
        {
            OrderId = orderId,
            Reason = null
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(Ok<>));
        
        var cancelledOrder = await context.Orders.FindAsync(orderId);
        cancelledOrder.ShouldNotBeNull();
        cancelledOrder.Status.ShouldBe("Cancelled");
        cancelledOrder.CancellationReason.ShouldBeNull();
    }
}
