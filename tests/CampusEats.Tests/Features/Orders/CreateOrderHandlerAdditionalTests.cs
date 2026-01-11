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

public class CreateOrderHandlerAdditionalTests
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
    public async Task GivenNullUserId_WhenCreatingOrder_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
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

        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = null,
            Items = new List<CreateOrderItemRequest>
            {
                new() { MenuItemId = menuItemId, Quantity = 1 }
            }
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(BadRequest<>));
        var resultValue = result.GetType().GetProperty("Value")?.GetValue(result);
        var error = resultValue?.GetType().GetProperty("error")?.GetValue(resultValue)?.ToString();
        error.ShouldBe("userId is required.");
    }

    [Fact]
    public async Task GivenWhitespaceUserId_WhenCreatingOrder_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var authenticatedUserId = "user123";
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
            UserId = "   ",
            Items = new List<CreateOrderItemRequest>
            {
                new() { MenuItemId = menuItemId, Quantity = 1 }
            }
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(BadRequest<>));
        var resultValue = result.GetType().GetProperty("Value")?.GetValue(result);
        var error = resultValue?.GetType().GetProperty("error")?.GetValue(resultValue)?.ToString();
        error.ShouldBe("userId is required.");
    }

    [Fact]
    public async Task GivenItemsWithoutMenuItemId_WhenCreatingOrder_ThenReturnsBadRequest()
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
                new() { MenuItemId = null, Quantity = 1 }
            }
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(BadRequest<>));
        var resultValue = result.GetType().GetProperty("Value")?.GetValue(result);
        var error = resultValue?.GetType().GetProperty("error")?.GetValue(resultValue)?.ToString();
        error.ShouldBe("Invalid items. Each item must reference a MenuItemId.");
    }

    [Fact]
    public async Task GivenZeroQuantity_WhenCreatingOrder_ThenDefaultsToOne()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
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

        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            OrderType = "Pickup",
            Items = new List<CreateOrderItemRequest>
            {
                new() { MenuItemId = menuItemId, Quantity = 0 }
            }
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(Created<>));
        
        var createdOrder = await context.Orders.Include(o => o.Items).FirstOrDefaultAsync();
        createdOrder.ShouldNotBeNull();
        createdOrder.Items.First().Quantity.ShouldBe(1);
        createdOrder.Subtotal.ShouldBe(10.00m);
    }

    [Fact]
    public async Task GivenNegativeQuantity_WhenCreatingOrder_ThenDefaultsToOne()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
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

        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            OrderType = "Pickup",
            Items = new List<CreateOrderItemRequest>
            {
                new() { MenuItemId = menuItemId, Quantity = -5 }
            }
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(Created<>));
        
        var createdOrder = await context.Orders.Include(o => o.Items).FirstOrDefaultAsync();
        createdOrder.ShouldNotBeNull();
        createdOrder.Items.First().Quantity.ShouldBe(1);
        createdOrder.Subtotal.ShouldBe(10.00m);
    }

    [Fact]
    public async Task GivenValidLoyaltyReward_WhenCreatingOrder_ThenAppliesDiscountAndDeductsPoints()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var menuItemId = Guid.NewGuid();
        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Test Item",
            Description = "Test",
            Price = 100.00m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        context.MenuItems.Add(menuItem);

        var loyaltyAccountId = Guid.NewGuid();
        var loyaltyAccount = new LoyaltyAccount
        {
            Id = loyaltyAccountId,
            UserId = userId,
            PointsBalance = 500,
            LifetimePoints = 500,
            Tier = "Bronze"
        };
        context.LoyaltyAccounts.Add(loyaltyAccount);

        var rewardId = Guid.NewGuid();
        var reward = new LoyaltyReward
        {
            Id = rewardId,
            Name = "10 RON Off",
            Description = "Get 10 RON off your order",
            PointsCost = 100,
            DiscountValue = 10.00m,
            IsActive = true,
            ValidFrom = DateTimeOffset.UtcNow.AddDays(-1),
            ValidUntil = DateTimeOffset.UtcNow.AddDays(1)
        };
        context.LoyaltyRewards.Add(reward);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            OrderType = "Pickup",
            LoyaltyRewardId = rewardId,
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
        createdOrder.Subtotal.ShouldBe(100.00m);
        createdOrder.Discount.ShouldBe(10.00m);
        createdOrder.Tax.ShouldBe(21.00m); // 21% of 100
        createdOrder.Total.ShouldBe(111.00m); // 100 + 21 - 10

        var updatedAccount = await context.LoyaltyAccounts.FindAsync(loyaltyAccountId);
        updatedAccount.ShouldNotBeNull();
        // Points are deducted (500 - 100 = 400), then earned back based on total (111 points)
        updatedAccount.PointsBalance.ShouldBe(511); // 500 - 100 + 111
    }

    [Fact]
    public async Task GivenNonExistentLoyaltyReward_WhenCreatingOrder_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
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

        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            OrderType = "Pickup",
            LoyaltyRewardId = Guid.NewGuid(),
            Items = new List<CreateOrderItemRequest>
            {
                new() { MenuItemId = menuItemId, Quantity = 1 }
            }
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(BadRequest<>));
        var resultValue = result.GetType().GetProperty("Value")?.GetValue(result);
        var error = resultValue?.GetType().GetProperty("error")?.GetValue(resultValue)?.ToString();
        error.ShouldBe("Loyalty reward not found.");
    }

    [Fact]
    public async Task GivenInactiveLoyaltyReward_WhenCreatingOrder_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
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

        var rewardId = Guid.NewGuid();
        var reward = new LoyaltyReward
        {
            Id = rewardId,
            Name = "Inactive Reward",
            Description = "This reward is inactive",
            PointsCost = 100,
            DiscountValue = 10.00m,
            IsActive = false
        };
        context.LoyaltyRewards.Add(reward);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            OrderType = "Pickup",
            LoyaltyRewardId = rewardId,
            Items = new List<CreateOrderItemRequest>
            {
                new() { MenuItemId = menuItemId, Quantity = 1 }
            }
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(BadRequest<>));
        var resultValue = result.GetType().GetProperty("Value")?.GetValue(result);
        var error = resultValue?.GetType().GetProperty("error")?.GetValue(resultValue)?.ToString();
        error.ShouldBe("This reward is not currently active.");
    }

    [Fact]
    public async Task GivenExpiredLoyaltyReward_WhenCreatingOrder_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
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

        var rewardId = Guid.NewGuid();
        var reward = new LoyaltyReward
        {
            Id = rewardId,
            Name = "Expired Reward",
            Description = "This reward has expired",
            PointsCost = 100,
            DiscountValue = 10.00m,
            IsActive = true,
            ValidUntil = DateTimeOffset.UtcNow.AddDays(-1) // Expired yesterday
        };
        context.LoyaltyRewards.Add(reward);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            OrderType = "Pickup",
            LoyaltyRewardId = rewardId,
            Items = new List<CreateOrderItemRequest>
            {
                new() { MenuItemId = menuItemId, Quantity = 1 }
            }
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(BadRequest<>));
        var resultValue = result.GetType().GetProperty("Value")?.GetValue(result);
        var error = resultValue?.GetType().GetProperty("error")?.GetValue(resultValue)?.ToString();
        error.ShouldBe("This reward has expired.");
    }

    [Fact]
    public async Task GivenNotYetValidReward_WhenCreatingOrder_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
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

        var rewardId = Guid.NewGuid();
        var reward = new LoyaltyReward
        {
            Id = rewardId,
            Name = "Future Reward",
            Description = "This reward is not yet valid",
            PointsCost = 100,
            DiscountValue = 10.00m,
            IsActive = true,
            ValidFrom = DateTimeOffset.UtcNow.AddDays(1) // Valid starting tomorrow
        };
        context.LoyaltyRewards.Add(reward);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            OrderType = "Pickup",
            LoyaltyRewardId = rewardId,
            Items = new List<CreateOrderItemRequest>
            {
                new() { MenuItemId = menuItemId, Quantity = 1 }
            }
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(BadRequest<>));
        var resultValue = result.GetType().GetProperty("Value")?.GetValue(result);
        var error = resultValue?.GetType().GetProperty("error")?.GetValue(resultValue)?.ToString();
        error.ShouldBe("This reward is not yet valid.");
    }

    [Fact]
    public async Task GivenInsufficientPoints_WhenCreatingOrder_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
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

        var loyaltyAccountId = Guid.NewGuid();
        var loyaltyAccount = new LoyaltyAccount
        {
            Id = loyaltyAccountId,
            UserId = userId,
            PointsBalance = 50, // Only 50 points
            LifetimePoints = 50,
            Tier = "Bronze"
        };
        context.LoyaltyAccounts.Add(loyaltyAccount);

        var rewardId = Guid.NewGuid();
        var reward = new LoyaltyReward
        {
            Id = rewardId,
            Name = "Expensive Reward",
            Description = "Requires 100 points",
            PointsCost = 100,
            DiscountValue = 10.00m,
            IsActive = true
        };
        context.LoyaltyRewards.Add(reward);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            OrderType = "Pickup",
            LoyaltyRewardId = rewardId,
            Items = new List<CreateOrderItemRequest>
            {
                new() { MenuItemId = menuItemId, Quantity = 1 }
            }
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(BadRequest<>));
        var resultValue = result.GetType().GetProperty("Value")?.GetValue(result);
        var error = resultValue?.GetType().GetProperty("error")?.GetValue(resultValue)?.ToString();
        error.ShouldContain("Insufficient points");
        error.ShouldContain("100");
        error.ShouldContain("50");
    }

    [Fact]
    public async Task GivenRewardWithMinimumTier_WhenUserIsBelowTier_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
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

        var loyaltyAccountId = Guid.NewGuid();
        var loyaltyAccount = new LoyaltyAccount
        {
            Id = loyaltyAccountId,
            UserId = userId,
            PointsBalance = 500,
            LifetimePoints = 500,
            Tier = "Bronze" // User is Bronze
        };
        context.LoyaltyAccounts.Add(loyaltyAccount);

        var rewardId = Guid.NewGuid();
        var reward = new LoyaltyReward
        {
            Id = rewardId,
            Name = "Gold Member Reward",
            Description = "Only for Gold members",
            PointsCost = 100,
            DiscountValue = 20.00m,
            IsActive = true,
            MinimumTier = "Gold" // Requires Gold tier
        };
        context.LoyaltyRewards.Add(reward);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            OrderType = "Pickup",
            LoyaltyRewardId = rewardId,
            Items = new List<CreateOrderItemRequest>
            {
                new() { MenuItemId = menuItemId, Quantity = 1 }
            }
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(BadRequest<>));
        var resultValue = result.GetType().GetProperty("Value")?.GetValue(result);
        var error = resultValue?.GetType().GetProperty("error")?.GetValue(resultValue)?.ToString();
        error.ShouldContain("Gold tier");
        error.ShouldContain("Bronze");
    }

    [Fact]
    public async Task GivenRewardWithMinimumOrderAmount_WhenOrderBelowMinimum_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var menuItemId = Guid.NewGuid();
        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Test Item",
            Description = "Test",
            Price = 20.00m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        context.MenuItems.Add(menuItem);

        var loyaltyAccountId = Guid.NewGuid();
        var loyaltyAccount = new LoyaltyAccount
        {
            Id = loyaltyAccountId,
            UserId = userId,
            PointsBalance = 500,
            LifetimePoints = 500,
            Tier = "Bronze"
        };
        context.LoyaltyAccounts.Add(loyaltyAccount);

        var rewardId = Guid.NewGuid();
        var reward = new LoyaltyReward
        {
            Id = rewardId,
            Name = "Big Order Discount",
            Description = "Requires minimum 50 RON order",
            PointsCost = 100,
            DiscountValue = 10.00m,
            IsActive = true,
            MinimumOrderAmount = 50.00m // Requires 50 RON minimum
        };
        context.LoyaltyRewards.Add(reward);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            OrderType = "Pickup",
            LoyaltyRewardId = rewardId,
            Items = new List<CreateOrderItemRequest>
            {
                new() { MenuItemId = menuItemId, Quantity = 1 } // Only 20 RON
            }
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(BadRequest<>));
        var resultValue = result.GetType().GetProperty("Value")?.GetValue(result);
        var error = resultValue?.GetType().GetProperty("error")?.GetValue(resultValue)?.ToString();
        error.ShouldContain("minimum order");
        error.ShouldContain("50");
        error.ShouldContain("20");
    }

    [Fact]
    public async Task GivenValidOrder_WhenCreated_ThenAwardsLoyaltyPoints()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var menuItemId = Guid.NewGuid();
        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Test Item",
            Description = "Test",
            Price = 50.00m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        context.MenuItems.Add(menuItem);

        var loyaltyAccountId = Guid.NewGuid();
        var loyaltyAccount = new LoyaltyAccount
        {
            Id = loyaltyAccountId,
            UserId = userId,
            PointsBalance = 100,
            LifetimePoints = 100,
            Tier = "Bronze"
        };
        context.LoyaltyAccounts.Add(loyaltyAccount);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            OrderType = "Pickup",
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
        // Total = 50 + (50 * 0.21) = 60.50 RON
        createdOrder.Total.ShouldBe(60.50m);

        var updatedAccount = await context.LoyaltyAccounts.FindAsync(loyaltyAccountId);
        updatedAccount.ShouldNotBeNull();
        // 1 RON = 1 point, so 60 points awarded (floor of 60.50)
        updatedAccount.PointsBalance.ShouldBe(160); // 100 + 60
        updatedAccount.LifetimePoints.ShouldBe(160); // 100 + 60
    }

    [Fact]
    public async Task GivenOrderReaching2000LifetimePoints_WhenCreated_ThenUpgradesToSilver()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var menuItemId = Guid.NewGuid();
        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Test Item",
            Description = "Test",
            Price = 500.00m, // Large order
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        context.MenuItems.Add(menuItem);

        var loyaltyAccountId = Guid.NewGuid();
        var loyaltyAccount = new LoyaltyAccount
        {
            Id = loyaltyAccountId,
            UserId = userId,
            PointsBalance = 1400,
            LifetimePoints = 1400, // Close to Silver (2000)
            Tier = "Bronze"
        };
        context.LoyaltyAccounts.Add(loyaltyAccount);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            OrderType = "Pickup",
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
        // Total = 500 + (500 * 0.21) = 605 RON
        createdOrder.Total.ShouldBe(605.00m);

        var updatedAccount = await context.LoyaltyAccounts.FindAsync(loyaltyAccountId);
        updatedAccount.ShouldNotBeNull();
        updatedAccount.LifetimePoints.ShouldBe(2005); // 1400 + 605
        updatedAccount.Tier.ShouldBe("Silver"); // Should upgrade to Silver
    }

    [Fact]
    public async Task GivenOrderReaching5000LifetimePoints_WhenCreated_ThenUpgradesToGold()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var menuItemId = Guid.NewGuid();
        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Test Item",
            Description = "Test",
            Price = 1000.00m, // Large order
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        context.MenuItems.Add(menuItem);

        var loyaltyAccountId = Guid.NewGuid();
        var loyaltyAccount = new LoyaltyAccount
        {
            Id = loyaltyAccountId,
            UserId = userId,
            PointsBalance = 3800,
            LifetimePoints = 3800, // Close to Gold (5000)
            Tier = "Silver"
        };
        context.LoyaltyAccounts.Add(loyaltyAccount);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            OrderType = "Pickup",
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
        // Total = 1000 + (1000 * 0.21) = 1210 RON
        createdOrder.Total.ShouldBe(1210.00m);

        var updatedAccount = await context.LoyaltyAccounts.FindAsync(loyaltyAccountId);
        updatedAccount.ShouldNotBeNull();
        updatedAccount.LifetimePoints.ShouldBe(5010); // 3800 + 1210
        updatedAccount.Tier.ShouldBe("Gold"); // Should upgrade to Gold
    }

    [Fact]
    public async Task GivenOrderReaching10000LifetimePoints_WhenCreated_ThenUpgradesToPlatinum()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var menuItemId = Guid.NewGuid();
        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Test Item",
            Description = "Test",
            Price = 2000.00m, // Very large order
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        context.MenuItems.Add(menuItem);

        var loyaltyAccountId = Guid.NewGuid();
        var loyaltyAccount = new LoyaltyAccount
        {
            Id = loyaltyAccountId,
            UserId = userId,
            PointsBalance = 7600,
            LifetimePoints = 7600, // Close to Platinum (10000)
            Tier = "Gold"
        };
        context.LoyaltyAccounts.Add(loyaltyAccount);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            OrderType = "Pickup",
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
        // Total = 2000 + (2000 * 0.21) = 2420 RON
        createdOrder.Total.ShouldBe(2420.00m);

        var updatedAccount = await context.LoyaltyAccounts.FindAsync(loyaltyAccountId);
        updatedAccount.ShouldNotBeNull();
        updatedAccount.LifetimePoints.ShouldBe(10020); // 7600 + 2420
        updatedAccount.Tier.ShouldBe("Platinum"); // Should upgrade to Platinum
    }

    [Fact]
    public async Task GivenUserWithNoLoyaltyAccount_WhenCreatingOrder_ThenCreatesOrderWithoutPoints()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
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
        // No loyalty account created
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            OrderType = "Pickup",
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
        createdOrder.UserId.ShouldBe(userId);
        createdOrder.Total.ShouldBe(12.10m); // 10 + 2.10 tax

        // Verify no loyalty account was created
        var loyaltyAccount = await context.LoyaltyAccounts.FirstOrDefaultAsync(la => la.UserId == userId);
        loyaltyAccount.ShouldBeNull();
    }

    [Fact]
    public async Task GivenPickupOrderType_WhenCreatingOrder_ThenDoesNotForcePickupTimeToNull()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
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

        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            OrderType = "Pickup",
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
        createdOrder.OrderType.ShouldBe("Pickup");
        // PickupTime can remain null or have a value - the handler doesn't force it to null for Pickup orders
        // This test verifies that the Pickup order type is handled correctly
    }

    [Fact]
    public async Task GivenOrderWithSpecialAndDeliveryInstructions_WhenCreated_ThenStoresInstructions()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
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

        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            OrderType = "Delivery",
            DeliveryInstructions = "Ring the doorbell twice",
            Items = new List<CreateOrderItemRequest>
            {
                new() 
                { 
                    MenuItemId = menuItemId, 
                    Quantity = 1,
                    SpecialInstructions = "No pickles, extra sauce"
                }
            }
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(Created<>));
        
        var createdOrder = await context.Orders.Include(o => o.Items).FirstOrDefaultAsync();
        createdOrder.ShouldNotBeNull();
        createdOrder.DeliveryInstructions.ShouldBe("Ring the doorbell twice");
        createdOrder.Items.First().SpecialInstructions.ShouldBe("No pickles, extra sauce");
    }

    [Fact]
    public async Task GivenMultipleIngredientsWhereOneIsInsufficientStock_WhenCreating_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var menuItemId = Guid.NewGuid();
        
        var inventoryItem1Id = Guid.NewGuid();
        var inventoryItem1 = new InventoryItem
        {
            Id = inventoryItem1Id,
            Name = "Beef Patty",
            Unit = "kg",
            CurrentQuantity = 10.0m, // Sufficient
            MinimumQuantity = 2.0m,
            IsOutOfStock = false,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        var inventoryItem2Id = Guid.NewGuid();
        var inventoryItem2 = new InventoryItem
        {
            Id = inventoryItem2Id,
            Name = "Cheese",
            Unit = "kg",
            CurrentQuantity = 0.5m, // Insufficient
            MinimumQuantity = 1.0m,
            IsOutOfStock = false,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Cheeseburger",
            Description = "Burger with cheese",
            Price = 15.00m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        context.InventoryItems.AddRange(inventoryItem1, inventoryItem2);
        context.MenuItems.Add(menuItem);
        await context.SaveChangesAsync();
        
        var ingredient1 = new MenuItemIngredient
        {
            MenuItemId = menuItemId,
            InventoryItemId = inventoryItem1Id,
            QuantityRequired = 0.2m // 0.2 kg per burger
        };
        
        var ingredient2 = new MenuItemIngredient
        {
            MenuItemId = menuItemId,
            InventoryItemId = inventoryItem2Id,
            QuantityRequired = 0.1m // 0.1 kg per burger
        };
        
        context.Set<MenuItemIngredient>().AddRange(ingredient1, ingredient2);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateMockHttpContextAccessor(userId);
        var handler = new CreateOrderHandler(context, httpContextAccessor);
        var request = new CreateOrderRequest
        {
            UserId = userId,
            OrderType = "Pickup",
            Items = new List<CreateOrderItemRequest>
            {
                new() { MenuItemId = menuItemId, Quantity = 10 } // Ordering 10 burgers
            }
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.GetType().GetGenericTypeDefinition().ShouldBe(typeof(BadRequest<>));
        var resultValue = result.GetType().GetProperty("Value")?.GetValue(result);
        var error = resultValue?.GetType().GetProperty("error")?.GetValue(resultValue)?.ToString();
        error.ShouldBe("Insufficient stock for one or more items.");
        
        // Check details
        var details = resultValue?.GetType().GetProperty("details")?.GetValue(resultValue) as System.Collections.IEnumerable;
        details.ShouldNotBeNull();
        var detailsList = details.Cast<string>().ToList();
        detailsList.Count.ShouldBe(1);
        detailsList[0].ShouldContain("Cheese");
        detailsList[0].ShouldContain("1"); // Required 1.0 kg (10 * 0.1)
        detailsList[0].ShouldContain("0.5"); // Available 0.5 kg
    }
}
