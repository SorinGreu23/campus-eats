using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.LoyaltyPoints.Get;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace CampusEats.Tests.Features.LoyaltyPoints;

public class GetLoyaltyPointsHandlerTests
{
    private static CampusDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new CampusDbContext(options);
    }

    [Fact]
    public async Task GivenNewUser_WhenGettingPoints_ThenCreatesNewAccountWithZeroPoints()
    {
        // Arrange
        await using var context = CreateContext();
        var handler = new GetLoyaltyPointsHandler(context);
        var userId = "newuser123";
        var request = new GetLoyaltyPointsRequest(userId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<LoyaltyPointsResponse>>();
        var okResult = (Ok<LoyaltyPointsResponse>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.UserId.ShouldBe(userId);
        okResult.Value.PointsBalance.ShouldBe(0);
        okResult.Value.LifetimePoints.ShouldBe(0);
        okResult.Value.Tier.ShouldBe("Bronze");

        var account = await context.LoyaltyAccounts.FirstOrDefaultAsync(a => a.UserId == userId);
        account.ShouldNotBeNull();
        account.PointsBalance.ShouldBe(0);
        account.LifetimePoints.ShouldBe(0);
        account.Tier.ShouldBe("Bronze");
    }

    [Fact]
    public async Task GivenExistingUserWithPoints_WhenGettingPoints_ThenReturnsCurrentBalance()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "existinguser123";
        var accountId = Guid.NewGuid();
        var existingAccount = new LoyaltyAccount
        {
            Id = accountId,
            UserId = userId,
            PointsBalance = 500,
            LifetimePoints = 800,
            Tier = "Silver"
        };
        context.LoyaltyAccounts.Add(existingAccount);
        await context.SaveChangesAsync();

        var handler = new GetLoyaltyPointsHandler(context);
        var request = new GetLoyaltyPointsRequest(userId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<LoyaltyPointsResponse>>();
        var okResult = (Ok<LoyaltyPointsResponse>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.AccountId.ShouldBe(accountId);
        okResult.Value.UserId.ShouldBe(userId);
        okResult.Value.PointsBalance.ShouldBe(500);
        okResult.Value.LifetimePoints.ShouldBe(800);
        okResult.Value.Tier.ShouldBe("Silver");
    }

    [Fact]
    public async Task GivenUserWithNullTier_WhenGettingPoints_ThenReturnsBronzeTier()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var existingAccount = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 100,
            LifetimePoints = 100,
            Tier = null
        };
        context.LoyaltyAccounts.Add(existingAccount);
        await context.SaveChangesAsync();

        var handler = new GetLoyaltyPointsHandler(context);
        var request = new GetLoyaltyPointsRequest(userId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<LoyaltyPointsResponse>>();
        var okResult = (Ok<LoyaltyPointsResponse>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Tier.ShouldBe("Bronze");
    }

    [Fact]
    public async Task GivenPlatinumUser_WhenGettingPoints_ThenReturnsPlatinumTier()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "platinumuser";
        var existingAccount = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 5000,
            LifetimePoints = 15000,
            Tier = "Platinum"
        };
        context.LoyaltyAccounts.Add(existingAccount);
        await context.SaveChangesAsync();

        var handler = new GetLoyaltyPointsHandler(context);
        var request = new GetLoyaltyPointsRequest(userId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<LoyaltyPointsResponse>>();
        var okResult = (Ok<LoyaltyPointsResponse>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Tier.ShouldBe("Platinum");
        okResult.Value.PointsBalance.ShouldBe(5000);
        okResult.Value.LifetimePoints.ShouldBe(15000);
    }
}
