using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.LoyaltyPoints.RedeemReward;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace CampusEats.Tests.Features.LoyaltyPoints;

public class RedeemRewardHandlerTests
{
    private static CampusDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new CampusDbContext(options);
    }

    [Fact]
    public async Task GivenNonExistentUser_WhenRedeemingReward_ThenReturnsNotFound()
    {
        // Arrange
        await using var context = CreateContext();
        var handler = new RedeemRewardHandler(context);
        var request = new RedeemRewardRequest
        {
            UserId = "nonexistent",
            RewardId = Guid.NewGuid()
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<NotFound<string>>();
        var notFound = (NotFound<string>)result;
        notFound.Value.ShouldBe("Loyalty account not found.");
    }

    [Fact]
    public async Task GivenNonExistentReward_WhenRedeemingReward_ThenReturnsNotFound()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 1000,
            LifetimePoints = 1000,
            Tier = "Silver"
        };
        context.LoyaltyAccounts.Add(account);
        await context.SaveChangesAsync();

        var handler = new RedeemRewardHandler(context);
        var request = new RedeemRewardRequest
        {
            UserId = userId,
            RewardId = Guid.NewGuid()
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<NotFound<string>>();
        var notFound = (NotFound<string>)result;
        notFound.Value.ShouldBe("Reward not found.");
    }

    [Fact]
    public async Task GivenInactiveReward_WhenRedeemingReward_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 1000,
            LifetimePoints = 1000,
            Tier = "Silver"
        };
        var reward = new LoyaltyReward
        {
            Id = Guid.NewGuid(),
            Name = "Inactive Reward",
            Description = "Not available",
            PointsCost = 100,
            DiscountValue = 10.00m,
            IsActive = false
        };
        context.LoyaltyAccounts.Add(account);
        context.LoyaltyRewards.Add(reward);
        await context.SaveChangesAsync();

        var handler = new RedeemRewardHandler(context);
        var request = new RedeemRewardRequest
        {
            UserId = userId,
            RewardId = reward.Id
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequest<string>>();
        var badRequest = (BadRequest<string>)result;
        badRequest.Value.ShouldBe("This reward is not currently available.");
    }

    [Fact]
    public async Task GivenInsufficientPoints_WhenRedeemingReward_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 50,
            LifetimePoints = 500,
            Tier = "Bronze"
        };
        var reward = new LoyaltyReward
        {
            Id = Guid.NewGuid(),
            Name = "Expensive Reward",
            Description = "Costs a lot",
            PointsCost = 100,
            DiscountValue = 10.00m,
            IsActive = true
        };
        context.LoyaltyAccounts.Add(account);
        context.LoyaltyRewards.Add(reward);
        await context.SaveChangesAsync();

        var handler = new RedeemRewardHandler(context);
        var request = new RedeemRewardRequest
        {
            UserId = userId,
            RewardId = reward.Id
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequest<string>>();
        var badRequest = (BadRequest<string>)result;
        badRequest.Value.ShouldNotBeNull();
        badRequest.Value.ShouldContain("Insufficient points");
    }

    [Fact]
    public async Task GivenInsufficientTier_WhenRedeemingReward_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 1000,
            LifetimePoints = 1000,
            Tier = "Bronze"
        };
        var reward = new LoyaltyReward
        {
            Id = Guid.NewGuid(),
            Name = "Gold Reward",
            Description = "Requires Gold tier",
            PointsCost = 100,
            DiscountValue = 10.00m,
            IsActive = true,
            MinimumTier = "Gold"
        };
        context.LoyaltyAccounts.Add(account);
        context.LoyaltyRewards.Add(reward);
        await context.SaveChangesAsync();

        var handler = new RedeemRewardHandler(context);
        var request = new RedeemRewardRequest
        {
            UserId = userId,
            RewardId = reward.Id
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequest<string>>();
        var badRequest = (BadRequest<string>)result;
        badRequest.Value.ShouldNotBeNull();
        badRequest.Value.ShouldContain("requires Gold tier");
    }

    [Fact]
    public async Task GivenRewardNotYetValid_WhenRedeemingReward_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 1000,
            LifetimePoints = 1000,
            Tier = "Silver"
        };
        var reward = new LoyaltyReward
        {
            Id = Guid.NewGuid(),
            Name = "Future Reward",
            Description = "Not valid yet",
            PointsCost = 100,
            DiscountValue = 10.00m,
            IsActive = true,
            ValidFrom = DateTimeOffset.UtcNow.AddDays(7)
        };
        context.LoyaltyAccounts.Add(account);
        context.LoyaltyRewards.Add(reward);
        await context.SaveChangesAsync();

        var handler = new RedeemRewardHandler(context);
        var request = new RedeemRewardRequest
        {
            UserId = userId,
            RewardId = reward.Id
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequest<string>>();
        var badRequest = (BadRequest<string>)result;
        badRequest.Value.ShouldBe("This reward is not yet valid.");
    }

    [Fact]
    public async Task GivenExpiredReward_WhenRedeemingReward_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 1000,
            LifetimePoints = 1000,
            Tier = "Silver"
        };
        var reward = new LoyaltyReward
        {
            Id = Guid.NewGuid(),
            Name = "Expired Reward",
            Description = "Already expired",
            PointsCost = 100,
            DiscountValue = 10.00m,
            IsActive = true,
            ValidUntil = DateTimeOffset.UtcNow.AddDays(-1)
        };
        context.LoyaltyAccounts.Add(account);
        context.LoyaltyRewards.Add(reward);
        await context.SaveChangesAsync();

        var handler = new RedeemRewardHandler(context);
        var request = new RedeemRewardRequest
        {
            UserId = userId,
            RewardId = reward.Id
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequest<string>>();
        var badRequest = (BadRequest<string>)result;
        badRequest.Value.ShouldBe("This reward has expired.");
    }
}
