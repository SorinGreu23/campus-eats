using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.LoyaltyPoints.GetRewards;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace CampusEats.Tests.Features.LoyaltyPoints;

public class GetRewardsHandlerTests
{
    private CampusDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new CampusDbContext(options);
    }

    [Fact]
    public async Task GivenNoRewards_WhenHandleIsCalled_ThenReturnsEmptyList()
    {
        // Arrange
        await using var context = CreateContext();
        var handler = new GetRewardsHandler(context);
        var request = new GetRewardsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<List<RewardResponse>>>();
        var okResult = (Ok<List<RewardResponse>>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GivenActiveRewards_WhenHandleIsCalled_ThenReturnsOnlyActiveRewards()
    {
        // Arrange
        await using var context = CreateContext();

        var activeReward = new LoyaltyReward
        {
            Id = Guid.NewGuid(),
            Name = "10% Off",
            Description = "Get 10% off your order",
            PointsCost = 100,
            DiscountValue = 10.00m,
            IsActive = true,
            ValidFrom = null,
            ValidUntil = null,
            MinimumTier = "Bronze"
        };

        var inactiveReward = new LoyaltyReward
        {
            Id = Guid.NewGuid(),
            Name = "20% Off",
            Description = "Get 20% off your order",
            PointsCost = 200,
            DiscountValue = 20.00m,
            IsActive = false,
            ValidFrom = null,
            ValidUntil = null,
            MinimumTier = "Silver"
        };

        context.LoyaltyRewards.AddRange(activeReward, inactiveReward);
        await context.SaveChangesAsync();

        var handler = new GetRewardsHandler(context);
        var request = new GetRewardsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<List<RewardResponse>>>();
        var okResult = (Ok<List<RewardResponse>>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Count.ShouldBe(1);
        okResult.Value[0].Name.ShouldBe("10% Off");
    }

    [Fact]
    public async Task GivenRewardsWithFutureValidFrom_WhenHandleIsCalled_ThenExcludesThoseRewards()
    {
        // Arrange
        await using var context = CreateContext();

        var currentReward = new LoyaltyReward
        {
            Id = Guid.NewGuid(),
            Name = "Current Reward",
            Description = "Valid now",
            PointsCost = 100,
            DiscountValue = 10.00m,
            IsActive = true,
            ValidFrom = DateTimeOffset.UtcNow.AddDays(-1),
            ValidUntil = null,
            MinimumTier = "Bronze"
        };

        var futureReward = new LoyaltyReward
        {
            Id = Guid.NewGuid(),
            Name = "Future Reward",
            Description = "Valid in the future",
            PointsCost = 200,
            DiscountValue = 20.00m,
            IsActive = true,
            ValidFrom = DateTimeOffset.UtcNow.AddDays(7),
            ValidUntil = null,
            MinimumTier = "Silver"
        };

        context.LoyaltyRewards.AddRange(currentReward, futureReward);
        await context.SaveChangesAsync();

        var handler = new GetRewardsHandler(context);
        var request = new GetRewardsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<List<RewardResponse>>>();
        var okResult = (Ok<List<RewardResponse>>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Count.ShouldBe(1);
        okResult.Value[0].Name.ShouldBe("Current Reward");
    }

    [Fact]
    public async Task GivenRewardsWithPastValidUntil_WhenHandleIsCalled_ThenExcludesExpiredRewards()
    {
        // Arrange
        await using var context = CreateContext();

        var validReward = new LoyaltyReward
        {
            Id = Guid.NewGuid(),
            Name = "Valid Reward",
            Description = "Still valid",
            PointsCost = 100,
            DiscountValue = 10.00m,
            IsActive = true,
            ValidFrom = null,
            ValidUntil = DateTimeOffset.UtcNow.AddDays(7),
            MinimumTier = "Bronze"
        };

        var expiredReward = new LoyaltyReward
        {
            Id = Guid.NewGuid(),
            Name = "Expired Reward",
            Description = "Expired yesterday",
            PointsCost = 200,
            DiscountValue = 20.00m,
            IsActive = true,
            ValidFrom = null,
            ValidUntil = DateTimeOffset.UtcNow.AddDays(-1),
            MinimumTier = "Silver"
        };

        context.LoyaltyRewards.AddRange(validReward, expiredReward);
        await context.SaveChangesAsync();

        var handler = new GetRewardsHandler(context);
        var request = new GetRewardsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<List<RewardResponse>>>();
        var okResult = (Ok<List<RewardResponse>>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Count.ShouldBe(1);
        okResult.Value[0].Name.ShouldBe("Valid Reward");
    }

    [Fact]
    public async Task GivenMultipleValidRewards_WhenHandleIsCalled_ThenReturnsAllValidRewards()
    {
        // Arrange
        await using var context = CreateContext();

        var reward1 = new LoyaltyReward
        {
            Id = Guid.NewGuid(),
            Name = "Free Drink",
            Description = "Get a free drink",
            PointsCost = 50,
            DiscountValue = 3.00m,
            IsActive = true,
            ValidFrom = null,
            ValidUntil = null,
            MinimumTier = "Bronze"
        };

        var reward2 = new LoyaltyReward
        {
            Id = Guid.NewGuid(),
            Name = "15% Off",
            Description = "Get 15% off",
            PointsCost = 150,
            DiscountValue = 15.00m,
            IsActive = true,
            ValidFrom = DateTimeOffset.UtcNow.AddDays(-5),
            ValidUntil = DateTimeOffset.UtcNow.AddDays(30),
            MinimumTier = "Silver"
        };

        var reward3 = new LoyaltyReward
        {
            Id = Guid.NewGuid(),
            Name = "Free Meal",
            Description = "Get a free meal",
            PointsCost = 500,
            DiscountValue = 12.99m,
            IsActive = true,
            ValidFrom = null,
            ValidUntil = null,
            MinimumTier = "Gold",
            MenuItemId = Guid.NewGuid()
        };

        context.LoyaltyRewards.AddRange(reward1, reward2, reward3);
        await context.SaveChangesAsync();

        var handler = new GetRewardsHandler(context);
        var request = new GetRewardsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<List<RewardResponse>>>();
        var okResult = (Ok<List<RewardResponse>>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Count.ShouldBe(3);
        okResult.Value.ShouldContain(r => r.Name == "Free Drink" && r.PointsCost == 50);
        okResult.Value.ShouldContain(r => r.Name == "15% Off" && r.PointsCost == 150);
        okResult.Value.ShouldContain(r => r.Name == "Free Meal" && r.PointsCost == 500);
    }
}
