using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.LoyaltyPoints.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace CampusEats.Tests.Features.LoyaltyPoints;

public class GetClaimsHandlerTests
{
    private CampusDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new CampusDbContext(options);
    }

    [Fact]
    public async Task GivenUserWithNoAccount_WhenGettingClaims_ThenReturnsEmptyList()
    {
        // Arrange
        await using var context = CreateContext();
        var handler = new GetClaimsHandler(context);
        var request = new GetClaimsRequest("nonexistent");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<List<ClaimResponse>>>();
        var okResult = (Ok<List<ClaimResponse>>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GivenUserWithNoClaims_WhenGettingClaims_ThenReturnsEmptyList()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 500,
            LifetimePoints = 500,
            Tier = "Bronze"
        };
        context.LoyaltyAccounts.Add(account);
        await context.SaveChangesAsync();

        var handler = new GetClaimsHandler(context);
        var request = new GetClaimsRequest(userId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<List<ClaimResponse>>>();
        var okResult = (Ok<List<ClaimResponse>>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GivenUserWithClaims_WhenGettingClaims_ThenReturnsAllClaims()
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
            LifetimePoints = 1500,
            Tier = "Silver"
        };

        var reward1Id = Guid.NewGuid();
        var reward1 = new LoyaltyReward
        {
            Id = reward1Id,
            Name = "10% Off",
            Description = "Get 10% off",
            PointsCost = 100,
            DiscountValue = 10.00m,
            IsActive = true
        };

        var reward2Id = Guid.NewGuid();
        var reward2 = new LoyaltyReward
        {
            Id = reward2Id,
            Name = "Free Drink",
            Description = "Get a free drink",
            PointsCost = 50,
            DiscountValue = 3.00m,
            IsActive = true
        };

        var claim1 = new LoyaltyClaim
        {
            Id = Guid.NewGuid(),
            LoyaltyAccountId = accountId,
            RewardId = reward1Id,
            ClaimedAt = DateTimeOffset.UtcNow.AddDays(-5),
            Notes = "First claim"
        };

        var claim2 = new LoyaltyClaim
        {
            Id = Guid.NewGuid(),
            LoyaltyAccountId = accountId,
            RewardId = reward2Id,
            ClaimedAt = DateTimeOffset.UtcNow.AddDays(-2),
            Notes = "Second claim"
        };

        context.LoyaltyAccounts.Add(account);
        context.LoyaltyRewards.AddRange(reward1, reward2);
        context.LoyaltyClaims.AddRange(claim1, claim2);
        await context.SaveChangesAsync();

        var handler = new GetClaimsHandler(context);
        var request = new GetClaimsRequest(userId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<List<ClaimResponse>>>();
        var okResult = (Ok<List<ClaimResponse>>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Count.ShouldBe(2);
        okResult.Value.ShouldContain(c => c.Reward != null && c.Reward.Name == "10% Off");
        okResult.Value.ShouldContain(c => c.Reward != null && c.Reward.Name == "Free Drink");
    }

    [Fact]
    public async Task GivenMultipleClaims_WhenGettingClaims_ThenReturnsOrderedByClaimedAtDescending()
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
            LifetimePoints = 1500,
            Tier = "Silver"
        };

        var rewardId = Guid.NewGuid();
        var reward = new LoyaltyReward
        {
            Id = rewardId,
            Name = "Test Reward",
            Description = "Test",
            PointsCost = 100,
            DiscountValue = 10.00m,
            IsActive = true
        };

        var oldClaim = new LoyaltyClaim
        {
            Id = Guid.NewGuid(),
            LoyaltyAccountId = accountId,
            RewardId = rewardId,
            ClaimedAt = DateTimeOffset.UtcNow.AddDays(-10),
            Notes = "Old claim"
        };

        var newClaim = new LoyaltyClaim
        {
            Id = Guid.NewGuid(),
            LoyaltyAccountId = accountId,
            RewardId = rewardId,
            ClaimedAt = DateTimeOffset.UtcNow.AddDays(-1),
            Notes = "Recent claim"
        };

        context.LoyaltyAccounts.Add(account);
        context.LoyaltyRewards.Add(reward);
        context.LoyaltyClaims.AddRange(oldClaim, newClaim);
        await context.SaveChangesAsync();

        var handler = new GetClaimsHandler(context);
        var request = new GetClaimsRequest(userId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<List<ClaimResponse>>>();
        var okResult = (Ok<List<ClaimResponse>>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Count.ShouldBe(2);
        okResult.Value[0].RedeemedAt.ShouldBe(newClaim.ClaimedAt);
        okResult.Value[1].RedeemedAt.ShouldBe(oldClaim.ClaimedAt);
    }

    [Fact]
    public async Task GivenClaimWithNullNotes_WhenGettingClaims_ThenReturnsClaimSuccessfully()
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
            Name = "Test Reward",
            Description = "Test",
            PointsCost = 100,
            DiscountValue = 10.00m,
            IsActive = true
        };

        var claim = new LoyaltyClaim
        {
            Id = Guid.NewGuid(),
            LoyaltyAccountId = accountId,
            RewardId = rewardId,
            ClaimedAt = DateTimeOffset.UtcNow,
            Notes = null
        };

        context.LoyaltyAccounts.Add(account);
        context.LoyaltyRewards.Add(reward);
        context.LoyaltyClaims.Add(claim);
        await context.SaveChangesAsync();

        var handler = new GetClaimsHandler(context);
        var request = new GetClaimsRequest(userId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<List<ClaimResponse>>>();
        var okResult = (Ok<List<ClaimResponse>>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Count.ShouldBe(1);
        okResult.Value[0].RewardId.ShouldBe(reward.Id);
    }
}
