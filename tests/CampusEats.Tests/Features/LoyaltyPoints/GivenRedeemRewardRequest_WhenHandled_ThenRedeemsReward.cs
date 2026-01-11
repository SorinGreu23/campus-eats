using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.LoyaltyPoints.RedeemReward;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace CampusEats.Tests.Features.LoyaltyPoints;

public class RedeemRewardHandlerTests
{
    private CampusDbContext CreateContext()
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

    [Fact]
    public async Task GivenValidRedemption_WhenRedeemingReward_ThenSuccessfullyRedeems()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var accountId = Guid.NewGuid();
        var account = new LoyaltyAccount
        {
            Id = accountId,
            UserId = userId,
            PointsBalance = 500,
            LifetimePoints = 1000,
            Tier = "Silver"
        };
        var rewardId = Guid.NewGuid();
        var reward = new LoyaltyReward
        {
            Id = rewardId,
            Name = "10% Off",
            Description = "Get 10% off",
            PointsCost = 100,
            DiscountValue = 10.00m,
            IsActive = true,
            MinimumTier = "Bronze"
        };
        context.LoyaltyAccounts.Add(account);
        context.LoyaltyRewards.Add(reward);
        await context.SaveChangesAsync();

        var handler = new RedeemRewardHandler(context);
        var request = new RedeemRewardRequest
        {
            UserId = userId,
            RewardId = rewardId,
            Reason = "Test redemption"
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<RedeemRewardResponse>>();
        var okResult = (Ok<RedeemRewardResponse>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.AccountId.ShouldBe(accountId);
        okResult.Value.NewPointsBalance.ShouldBe(500); // Points NOT deducted yet - deducted at order placement
        okResult.Value.Message.ShouldContain("Successfully redeemed");

        var updatedAccount = await context.LoyaltyAccounts.FindAsync(accountId);
        updatedAccount.ShouldNotBeNull();
        updatedAccount.PointsBalance.ShouldBe(500); // Points NOT deducted yet

        var claim = await context.LoyaltyClaims.FirstOrDefaultAsync(c => c.RewardId == rewardId);
        claim.ShouldNotBeNull();
        claim.LoyaltyAccountId.ShouldBe(accountId);
        claim.Notes.ShouldBe("Test redemption");
    }

    [Fact]
    public async Task GivenSilverUserRedeemingSilverReward_WhenRedeemingReward_ThenSucceeds()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 500,
            LifetimePoints = 1500,
            Tier = "Silver"
        };
        var reward = new LoyaltyReward
        {
            Id = Guid.NewGuid(),
            Name = "Silver Reward",
            Description = "For silver members",
            PointsCost = 200,
            DiscountValue = 15.00m,
            IsActive = true,
            MinimumTier = "Silver"
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
        result.ShouldBeOfType<Ok<RedeemRewardResponse>>();
        var okResult = (Ok<RedeemRewardResponse>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.NewPointsBalance.ShouldBe(500); // Points NOT deducted yet - deducted at order placement
    }

    [Fact]
    public async Task GivenUserBelowMinimumTier_WhenRedeemingReward_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "bronze-user";
        var account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 5000, // Plenty of points
            LifetimePoints = 5000,
            Tier = "Bronze" // But wrong tier
        };
        var reward = new LoyaltyReward
        {
            Id = Guid.NewGuid(),
            Name = "Platinum Exclusive",
            Description = "Only for Platinum members",
            PointsCost = 1000,
            DiscountValue = 50.00m,
            IsActive = true,
            MinimumTier = "Platinum" // Requires Platinum
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
        badRequest.Value.ShouldContain("requires Platinum tier");
        
        // Verify no claim was created
        var claimCount = await context.LoyaltyClaims.CountAsync();
        claimCount.ShouldBe(0);
    }

    [Fact]
    public async Task GivenNonExistentReward_WhenRedeeming_ThenReturnsNotFound()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "valid-user";
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

        var nonExistentRewardId = Guid.NewGuid();
        var handler = new RedeemRewardHandler(context);
        var request = new RedeemRewardRequest
        {
            UserId = userId,
            RewardId = nonExistentRewardId
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<NotFound<string>>();
        var notFound = (NotFound<string>)result;
        notFound.Value.ShouldBe("Reward not found.");
        
        // Verify account points unchanged
        var unchangedAccount = await context.LoyaltyAccounts.FindAsync(account.Id);
        unchangedAccount.ShouldNotBeNull();
        unchangedAccount!.PointsBalance.ShouldBe(1000);
    }
}
