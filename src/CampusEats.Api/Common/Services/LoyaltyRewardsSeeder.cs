using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Common.Services;

public static class LoyaltyRewardsSeeder
{
    public static async Task SeedLoyaltyRewards(CampusDbContext context)
    {
        // Check if rewards need to be updated with tier information
        var rewardsWithoutTier = await context
            .LoyaltyRewards.Where(r => r.MinimumTier == null)
            .ToListAsync();

        if (rewardsWithoutTier.Count != 0)
        {
            // Clear old rewards without tier info and reseed
            context.LoyaltyRewards.RemoveRange(rewardsWithoutTier);
            await context.SaveChangesAsync();
        }
        else if (await context.LoyaltyRewards.AnyAsync())
        {
            return; // Already seeded with tier information
        }

        var rewards = new List<LoyaltyReward>
        {
            new LoyaltyReward
            {
                Id = Guid.NewGuid(),
                Name = "Free Drink",
                Description = "Get any drink for free with your next order",
                PointsCost = 100,
                DiscountValue = 5.00m,
                IsActive = true,
                MinimumTier = "Bronze",
                ValidFrom = DateTimeOffset.UtcNow,
                ValidUntil = DateTimeOffset.UtcNow.AddYears(1),
            },
            new LoyaltyReward
            {
                Id = Guid.NewGuid(),
                Name = "$5 Off",
                Description = "Get $5 off your next order",
                PointsCost = 250,
                DiscountValue = 5.00m,
                IsActive = true,
                MinimumTier = "Bronze",
                ValidFrom = DateTimeOffset.UtcNow,
                ValidUntil = DateTimeOffset.UtcNow.AddYears(1),
            },
            new LoyaltyReward
            {
                Id = Guid.NewGuid(),
                Name = "$10 Off",
                Description = "Get $10 off your next order - Silver+ Only",
                PointsCost = 500,
                DiscountValue = 10.00m,
                IsActive = true,
                MinimumTier = "Silver",
                ValidFrom = DateTimeOffset.UtcNow,
                ValidUntil = DateTimeOffset.UtcNow.AddYears(1),
            },
            new LoyaltyReward
            {
                Id = Guid.NewGuid(),
                Name = "Free Dessert",
                Description = "Get a free dessert with your next meal",
                PointsCost = 150,
                DiscountValue = 4.00m,
                IsActive = true,
                MinimumTier = "Bronze",
                ValidFrom = DateTimeOffset.UtcNow,
                ValidUntil = DateTimeOffset.UtcNow.AddYears(1),
            },
            new LoyaltyReward
            {
                Id = Guid.NewGuid(),
                Name = "$15 Off - Gold Exclusive",
                Description = "Exclusive $15 discount for Gold members and above",
                PointsCost = 750,
                DiscountValue = 15.00m,
                IsActive = true,
                MinimumTier = "Gold",
                ValidFrom = DateTimeOffset.UtcNow,
                ValidUntil = DateTimeOffset.UtcNow.AddYears(1),
            },
            new LoyaltyReward
            {
                Id = Guid.NewGuid(),
                Name = "$25 Off - Platinum Special",
                Description = "Exclusive $25 discount for platinum members",
                PointsCost = 1000,
                DiscountValue = 25.00m,
                IsActive = true,
                MinimumTier = "Platinum",
                ValidFrom = DateTimeOffset.UtcNow,
                ValidUntil = DateTimeOffset.UtcNow.AddYears(1),
            },
        };

        context.LoyaltyRewards.AddRange(rewards);
        await context.SaveChangesAsync();
    }
}
