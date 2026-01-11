using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.LoyaltyPoints.AddPoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace CampusEats.Tests.Features.LoyaltyPoints;

public class AddPointsHandlerTests
{
    private static CampusDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new CampusDbContext(options);
    }

    [Fact]
    public async Task GivenZeroPoints_WhenHandleIsCalled_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var handler = new AddPointsHandler(context);
        var request = new AddPointsRequest { UserId = "user123", Points = 0 };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequest<string>>();
        var badRequest = (BadRequest<string>)result;
        badRequest.Value.ShouldBe("Points must be greater than zero.");
    }

    [Fact]
    public async Task GivenNegativePoints_WhenHandleIsCalled_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = CreateContext();
        var handler = new AddPointsHandler(context);
        var request = new AddPointsRequest { UserId = "user123", Points = -10 };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<BadRequest<string>>();
        var badRequest = (BadRequest<string>)result;
        badRequest.Value.ShouldBe("Points must be greater than zero.");
    }

    [Fact]
    public async Task GivenNewUser_WhenAddingPoints_ThenCreatesNewLoyaltyAccount()
    {
        // Arrange
        await using var context = CreateContext();
        var handler = new AddPointsHandler(context);
        var userId = "newuser123";
        var request = new AddPointsRequest { UserId = userId, Points = 100 };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<AddPointsResponse>>();
        var okResult = (Ok<AddPointsResponse>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.PointsAdded.ShouldBe(100);
        okResult.Value.NewPointsBalance.ShouldBe(100);
        okResult.Value.LifetimePoints.ShouldBe(100);
        okResult.Value.Tier.ShouldBe("Bronze");

        var account = await context.LoyaltyAccounts.FirstOrDefaultAsync(a => a.UserId == userId);
        account.ShouldNotBeNull();
        account.PointsBalance.ShouldBe(100);
        account.LifetimePoints.ShouldBe(100);
        account.Tier.ShouldBe("Bronze");
    }

    [Fact]
    public async Task GivenExistingUser_WhenAddingPoints_ThenUpdatesPointsBalance()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "existinguser123";
        var existingAccount = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 200,
            LifetimePoints = 200,
            Tier = "Bronze"
        };
        context.LoyaltyAccounts.Add(existingAccount);
        await context.SaveChangesAsync();

        var handler = new AddPointsHandler(context);
        var request = new AddPointsRequest { UserId = userId, Points = 150 };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<AddPointsResponse>>();
        var okResult = (Ok<AddPointsResponse>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.PointsAdded.ShouldBe(150);
        okResult.Value.NewPointsBalance.ShouldBe(350);
        okResult.Value.LifetimePoints.ShouldBe(350);
        okResult.Value.Tier.ShouldBe("Bronze");
    }

    [Fact]
    public async Task GivenPointsCrossSilverThreshold_WhenAddingPoints_ThenUpgradesToSilver()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var existingAccount = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 900,
            LifetimePoints = 900,
            Tier = "Bronze"
        };
        context.LoyaltyAccounts.Add(existingAccount);
        await context.SaveChangesAsync();

        var handler = new AddPointsHandler(context);
        var request = new AddPointsRequest { UserId = userId, Points = 200 };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<AddPointsResponse>>();
        var okResult = (Ok<AddPointsResponse>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Tier.ShouldBe("Silver");
        okResult.Value.LifetimePoints.ShouldBe(1100);
    }

    [Fact]
    public async Task GivenPointsCrossGoldThreshold_WhenAddingPoints_ThenUpgradesToGold()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var existingAccount = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 4500,
            LifetimePoints = 4500,
            Tier = "Silver"
        };
        context.LoyaltyAccounts.Add(existingAccount);
        await context.SaveChangesAsync();

        var handler = new AddPointsHandler(context);
        var request = new AddPointsRequest { UserId = userId, Points = 600 };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<AddPointsResponse>>();
        var okResult = (Ok<AddPointsResponse>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Tier.ShouldBe("Gold");
        okResult.Value.LifetimePoints.ShouldBe(5100);
    }

    [Fact]
    public async Task GivenPointsCrossPlatinumThreshold_WhenAddingPoints_ThenUpgradesToPlatinum()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "user123";
        var existingAccount = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 9500,
            LifetimePoints = 9500,
            Tier = "Gold"
        };
        context.LoyaltyAccounts.Add(existingAccount);
        await context.SaveChangesAsync();

        var handler = new AddPointsHandler(context);
        var request = new AddPointsRequest { UserId = userId, Points = 600 };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<AddPointsResponse>>();
        var okResult = (Ok<AddPointsResponse>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Tier.ShouldBe("Platinum");
        okResult.Value.LifetimePoints.ShouldBe(10100);
    }

    [Fact]
    public async Task GivenUserWithNoAccount_WhenAddingPoints_ThenCreatesAccount()
    {
        // Arrange
        await using var context = CreateContext();
        var userId = "brandnewuser";
        
        // Verify no account exists
        var existingAccount = await context.LoyaltyAccounts
            .FirstOrDefaultAsync(a => a.UserId == userId);
        existingAccount.ShouldBeNull();

        var handler = new AddPointsHandler(context);
        var request = new AddPointsRequest { UserId = userId, Points = 250 };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<AddPointsResponse>>();
        var okResult = (Ok<AddPointsResponse>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.PointsAdded.ShouldBe(250);
        okResult.Value.NewPointsBalance.ShouldBe(250);
        okResult.Value.LifetimePoints.ShouldBe(250);
        okResult.Value.Tier.ShouldBe("Bronze");

        // Verify account was created in database
        var newAccount = await context.LoyaltyAccounts
            .FirstOrDefaultAsync(a => a.UserId == userId);
        newAccount.ShouldNotBeNull();
        newAccount!.UserId.ShouldBe(userId);
        newAccount.PointsBalance.ShouldBe(250);
        newAccount.LifetimePoints.ShouldBe(250);
        newAccount.Tier.ShouldBe("Bronze");
    }

    [Fact]
    public async Task GivenPointsAddition_WhenTierThresholdMet_ThenUpgradesTier()
    {
        // Arrange - User at 1950 points (just below Silver threshold of 2000)
        await using var context = CreateContext();
        var userId = "user-at-threshold";
        var existingAccount = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 1950,
            LifetimePoints = 1950,
            Tier = "Bronze"
        };
        context.LoyaltyAccounts.Add(existingAccount);
        await context.SaveChangesAsync();

        var handler = new AddPointsHandler(context);
        var request = new AddPointsRequest { UserId = userId, Points = 100 }; // Should cross to 2050

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<AddPointsResponse>>();
        var okResult = (Ok<AddPointsResponse>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.PointsAdded.ShouldBe(100);
        okResult.Value.NewPointsBalance.ShouldBe(2050);
        okResult.Value.LifetimePoints.ShouldBe(2050);
        okResult.Value.Tier.ShouldBe("Silver"); // Upgraded!

        // Verify tier upgrade persisted
        var updatedAccount = await context.LoyaltyAccounts
            .FirstOrDefaultAsync(a => a.UserId == userId);
        updatedAccount.ShouldNotBeNull();
        updatedAccount!.Tier.ShouldBe("Silver");
    }

    [Fact]
    public async Task GivenMultipleTierThresholds_WhenAddingLargePoints_ThenUpgradesToCorrectTier()
    {
        // Arrange - User at 100 points, adding 10000 should jump to Platinum
        await using var context = CreateContext();
        var userId = "big-spender";
        var existingAccount = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 100,
            LifetimePoints = 100,
            Tier = "Bronze"
        };
        context.LoyaltyAccounts.Add(existingAccount);
        await context.SaveChangesAsync();

        var handler = new AddPointsHandler(context);
        var request = new AddPointsRequest { UserId = userId, Points = 10000 };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<AddPointsResponse>>();
        var okResult = (Ok<AddPointsResponse>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.LifetimePoints.ShouldBe(10100);
        okResult.Value.Tier.ShouldBe("Platinum"); // Should skip Silver and Gold directly to Platinum
    }
}
