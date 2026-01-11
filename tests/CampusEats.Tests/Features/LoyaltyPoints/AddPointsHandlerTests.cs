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
}
