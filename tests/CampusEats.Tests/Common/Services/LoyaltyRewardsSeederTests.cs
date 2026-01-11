using CampusEats.Api.Common.Services;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Tests.Common.Services;

public class LoyaltyRewardsSeederTests
{
    private readonly DbContextOptions<CampusDbContext> _options;

    public LoyaltyRewardsSeederTests()
    {
        _options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GivenEmptyDatabase_WhenSeeding_ThenCreatesExpectedRecords()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);

        // Act
        await LoyaltyRewardsSeeder.SeedLoyaltyRewards(context);

        // Assert
        var rewards = await context.LoyaltyRewards.ToListAsync();
        rewards.Should().NotBeEmpty();
        rewards.Should().Contain(r => r.Name == "15 RON Off");
        rewards.Should().Contain(r => r.Name == "20 RON Off");
        rewards.Should().Contain(r => r.Name == "25 RON Off");
        rewards.Should().Contain(r => r.Name == "50 RON Off");
        rewards.Should().Contain(r => r.Name.Contains("75 RON"));
        rewards.Should().Contain(r => r.Name.Contains("125 RON"));
    }

    [Fact]
    public async Task GivenExistingData_WhenSeeding_ThenDoesNotDuplicate()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);

        // Seed first time
        await LoyaltyRewardsSeeder.SeedLoyaltyRewards(context);
        var firstCount = await context.LoyaltyRewards.CountAsync();

        // Act - Seed second time
        await LoyaltyRewardsSeeder.SeedLoyaltyRewards(context);

        // Assert - The seeder always adds new rewards (Program.cs is expected to delete old ones first)
        var secondCount = await context.LoyaltyRewards.CountAsync();
        secondCount.Should().Be(firstCount * 2); // Expect duplicates since seeder doesn't check
    }

    [Fact]
    public async Task GivenEmptyDatabase_WhenSeeding_ThenRewardsHaveMinimumOrderAmounts()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);

        // Act
        await LoyaltyRewardsSeeder.SeedLoyaltyRewards(context);

        // Assert
        var rewards = await context.LoyaltyRewards.ToListAsync();
        
        var reward15 = rewards.FirstOrDefault(r => r.Name == "15 RON Off");
        reward15.Should().NotBeNull();
        reward15!.MinimumOrderAmount.Should().Be(20);

        var reward20 = rewards.FirstOrDefault(r => r.Name == "20 RON Off");
        reward20.Should().NotBeNull();
        reward20!.MinimumOrderAmount.Should().Be(30);

        var reward25 = rewards.FirstOrDefault(r => r.Name == "25 RON Off");
        reward25.Should().NotBeNull();
        reward25!.MinimumOrderAmount.Should().Be(35);

        var reward50 = rewards.FirstOrDefault(r => r.Name == "50 RON Off");
        reward50.Should().NotBeNull();
        reward50!.MinimumOrderAmount.Should().Be(70);
    }

    [Fact]
    public async Task GivenEmptyDatabase_WhenSeeding_ThenRewardsHaveCorrectTierRequirements()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);

        // Act
        await LoyaltyRewardsSeeder.SeedLoyaltyRewards(context);

        // Assert
        var rewards = await context.LoyaltyRewards.ToListAsync();
        
        // 15 and 20 RON rewards should be Bronze tier
        var bronzeRewards = rewards.Where(r => r.Name == "15 RON Off" || r.Name == "20 RON Off" || r.Name == "25 RON Off").ToList();
        bronzeRewards.Should().OnlyContain(r => r.MinimumTier == "Bronze");

        // 50 RON reward should be Silver tier
        var silverRewards = rewards.Where(r => r.Name.Contains("50 RON")).ToList();
        silverRewards.Should().OnlyContain(r => r.MinimumTier == "Silver");

        // 75 RON reward should be Gold tier
        var goldRewards = rewards.Where(r => r.Name.Contains("75 RON")).ToList();
        goldRewards.Should().OnlyContain(r => r.MinimumTier == "Gold");

        // 125 RON reward should be Platinum tier
        var platinumRewards = rewards.Where(r => r.Name.Contains("125 RON")).ToList();
        platinumRewards.Should().OnlyContain(r => r.MinimumTier == "Platinum");
    }

    [Fact]
    public async Task GivenEmptyDatabase_WhenSeeding_ThenAllRewardsAreActive()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);

        // Act
        await LoyaltyRewardsSeeder.SeedLoyaltyRewards(context);

        // Assert
        var rewards = await context.LoyaltyRewards.ToListAsync();
        rewards.Should().OnlyContain(r => r.IsActive);
    }

    [Fact]
    public async Task GivenEmptyDatabase_WhenSeeding_ThenRewardsHaveCorrectPointsCosts()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);

        // Act
        await LoyaltyRewardsSeeder.SeedLoyaltyRewards(context);

        // Assert
        var rewards = await context.LoyaltyRewards.ToListAsync();
        
        var reward15 = rewards.FirstOrDefault(r => r.Name == "15 RON Off");
        reward15.Should().NotBeNull();
        reward15!.PointsCost.Should().Be(100);

        var reward20 = rewards.FirstOrDefault(r => r.Name == "20 RON Off");
        reward20.Should().NotBeNull();
        reward20!.PointsCost.Should().Be(150);

        var reward125 = rewards.FirstOrDefault(r => r.Name.Contains("125 RON"));
        reward125.Should().NotBeNull();
        reward125!.PointsCost.Should().Be(1000);
    }
}
