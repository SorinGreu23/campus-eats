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
    private static CampusDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new CampusDbContext(options);
    }

    private static IHttpContextAccessor CreateMockHttpContextAccessor(string userId, bool isAuthenticated = true)
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
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(BadRequest<>));
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
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(BadRequest<>));
    }

    [Fact]
    public async Task GivenMismatchedUserId_WhenCreatingOrder_ThenReturnsForbid()
    {
        // Arrange
        await using var context = CreateContext();
        var authenticatedUserId = "user123";
        var requestUserId = "user456";
        var menuItemId = Guid.NewGuid();
        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Test Item",
            Description = "Test",
            Price = 10.00m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        context.MenuItems.Add(menuItem);
        await context.SaveChangesAsync();
        
        var httpContextAccessor = CreateMockHttpContextAccessor(authenticatedUserId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = requestUserId,
            OrderType = "Pickup",
            Items = new List<CreateOrderItemRequest>
            {
                new() { MenuItemId = menuItemId, Quantity = 1 }
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
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(BadRequest<>));
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
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(Created<>));
        
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
            OrderType = "Pickup",
            Items = new List<CreateOrderItemRequest>
            {
                new() { MenuItemId = menuItem1Id, Quantity = 2 },
                new() { MenuItemId = menuItem2Id, Quantity = 3 }
            }
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(Created<>));
        
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
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(Created<>));
        
        var createdOrder = await context.Orders.FirstOrDefaultAsync();
        createdOrder.ShouldNotBeNull();
        createdOrder.OrderType.ShouldBe("Delivery");
        createdOrder.PickupTime.ShouldBeNull();
    }

    [Fact]
    public async Task GivenInsufficientStock_WhenCreatingOrder_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var menuItemId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();

        var inventoryItem = new InventoryItem
        {
            Id = inventoryItemId,
            Name = "Beef Patty",
            Unit = "kg",
            CurrentQuantity = 1m, // Only 1kg available
            MinimumQuantity = 0.5m,
            IsOutOfStock = false,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Burger",
            Description = "Beef burger",
            Price = 10.00m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var menuItemIngredient = new MenuItemIngredient
        {
            MenuItemId = menuItemId,
            InventoryItemId = inventoryItemId,
            QuantityRequired = 0.25m // Requires 0.25kg per burger
        };

        context.InventoryItems.Add(inventoryItem);
        context.MenuItems.Add(menuItem);
        await context.SaveChangesAsync();
        
        context.MenuItemIngredients.Add(menuItemIngredient);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            Items = new List<CreateOrderItemRequest>
            {
                new() { MenuItemId = menuItemId, Quantity = 5 } // Trying to order 5 burgers (needs 1.25kg, but only 1kg available)
            }
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(BadRequest<>));
        
        // Verify no order was created
        var orderCount = await context.Orders.CountAsync();
        orderCount.ShouldBe(0);
    }

    [Fact]
    public async Task GivenSufficientStock_WhenCreatingOrder_ThenCreatesOrder()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var menuItemId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();

        var inventoryItem = new InventoryItem
        {
            Id = inventoryItemId,
            Name = "Beef Patty",
            Unit = "kg",
            CurrentQuantity = 100m, // Plenty available
            MinimumQuantity = 10m,
            IsOutOfStock = false,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Burger",
            Description = "Beef burger",
            Price = 10.00m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var menuItemIngredient = new MenuItemIngredient
        {
            MenuItemId = menuItemId,
            InventoryItemId = inventoryItemId,
            QuantityRequired = 0.25m // Requires 0.25kg per burger
        };

        context.InventoryItems.Add(inventoryItem);
        context.MenuItems.Add(menuItem);
        await context.SaveChangesAsync();
        
        context.MenuItemIngredients.Add(menuItemIngredient);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            Items = new List<CreateOrderItemRequest>
            {
                new() { MenuItemId = menuItemId, Quantity = 2 } // Ordering 2 burgers (needs 0.5kg, plenty available)
            }
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(Created<>));
        
        // Verify order was created
        var createdOrder = await context.Orders.Include(o => o.Items).FirstOrDefaultAsync();
        createdOrder.ShouldNotBeNull();
        createdOrder.Items.Count.ShouldBe(1);
        createdOrder.Items.First().Quantity.ShouldBe(2);
    }

    [Fact]
    public async Task GivenItemWithNoIngredients_WhenCreatingOrder_ThenCreatesOrder()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var menuItemId = Guid.NewGuid();

        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Coffee",
            Description = "Black coffee (no inventory tracking)",
            Price = 2.99m,
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
            Items = new List<CreateOrderItemRequest>
            {
                new() { MenuItemId = menuItemId, Quantity = 10 } // No ingredients, so any quantity is allowed
            }
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(Created<>));
        
        // Verify order was created
        var createdOrder = await context.Orders.Include(o => o.Items).FirstOrDefaultAsync();
        createdOrder.ShouldNotBeNull();
        createdOrder.Items.Count.ShouldBe(1);
        createdOrder.Items.First().Quantity.ShouldBe(10);
    }

    [Fact]
    public async Task GivenExactStockMatch_WhenOrderingOne_ThenCreatesOrder()
    {
        // Arrange: 1kg chicken breast available, 1 item requires 1kg
        await using var context = CreateContext();
        var userId = "user123";
        var menuItemId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();

        var inventoryItem = new InventoryItem
        {
            Id = inventoryItemId,
            Name = "Chicken Breast",
            Unit = "kg",
            CurrentQuantity = 1m, // Exactly 1kg available
            MinimumQuantity = 0.5m,
            IsOutOfStock = false,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Chicken Dish",
            Description = "Grilled chicken",
            Price = 15.00m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var menuItemIngredient = new MenuItemIngredient
        {
            MenuItemId = menuItemId,
            InventoryItemId = inventoryItemId,
            QuantityRequired = 1m // Requires exactly 1kg per item
        };

        context.InventoryItems.Add(inventoryItem);
        context.MenuItems.Add(menuItem);
        await context.SaveChangesAsync();
        
        context.MenuItemIngredients.Add(menuItemIngredient);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            Items = new List<CreateOrderItemRequest>
            {
                new() { MenuItemId = menuItemId, Quantity = 1 } // Order exactly 1 item
            }
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(Created<>));
        
        var createdOrder = await context.Orders.Include(o => o.Items).FirstOrDefaultAsync();
        createdOrder.ShouldNotBeNull();
        createdOrder.Items.Count.ShouldBe(1);
        createdOrder.Items.First().Quantity.ShouldBe(1);
    }

    [Fact]
    public async Task GivenExactStockMatch_WhenOrderingTwo_ThenReturnsBadRequest()
    {
        // Arrange: 1kg chicken breast available, 1 item requires 1kg, trying to order 2
        await using var context = CreateContext();
        var userId = "user123";
        var menuItemId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();

        var inventoryItem = new InventoryItem
        {
            Id = inventoryItemId,
            Name = "Chicken Breast",
            Unit = "kg",
            CurrentQuantity = 1m, // Exactly 1kg available
            MinimumQuantity = 0.5m,
            IsOutOfStock = false,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Chicken Dish",
            Description = "Grilled chicken",
            Price = 15.00m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var menuItemIngredient = new MenuItemIngredient
        {
            MenuItemId = menuItemId,
            InventoryItemId = inventoryItemId,
            QuantityRequired = 1m // Requires exactly 1kg per item
        };

        context.InventoryItems.Add(inventoryItem);
        context.MenuItems.Add(menuItem);
        await context.SaveChangesAsync();
        
        context.MenuItemIngredients.Add(menuItemIngredient);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            Items = new List<CreateOrderItemRequest>
            {
                new() { MenuItemId = menuItemId, Quantity = 2 } // Try to order 2 items (would need 2kg)
            }
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert - Should be rejected because we need 2kg but only have 1kg
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(BadRequest<>));
        
        // Verify no order was created
        var orderCount = await context.Orders.CountAsync();
        orderCount.ShouldBe(0);
    }
}
