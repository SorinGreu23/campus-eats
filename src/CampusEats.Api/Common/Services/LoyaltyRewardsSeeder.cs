using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Common.Services;

public static class LoyaltyRewardsSeeder
{
    public static async Task SeedLoyaltyRewards(CampusDbContext context)
    {
        // Always seed rewards (assuming old ones were deleted in Program.cs)
        var rewards = new List<LoyaltyReward>
        {
            new LoyaltyReward
            {
                Id = Guid.NewGuid(),
                Name = "15 RON Off",
                Description = "Get 15 RON off your next order (min. 20 RON)",
                PointsCost = 100,
                DiscountValue = 15.00m,
                IsActive = true,
                MinimumTier = "Bronze",
                MinimumOrderAmount = 20.00m,
                ValidFrom = DateTimeOffset.UtcNow,
                ValidUntil = DateTimeOffset.UtcNow.AddYears(1),
            },
            new LoyaltyReward
            {
                Id = Guid.NewGuid(),
                Name = "25 RON Off",
                Description = "Get 25 RON off your next order (min. 35 RON)",
                PointsCost = 250,
                DiscountValue = 25.00m,
                IsActive = true,
                MinimumTier = "Bronze",
                MinimumOrderAmount = 35.00m,
                ValidFrom = DateTimeOffset.UtcNow,
                ValidUntil = DateTimeOffset.UtcNow.AddYears(1),
            },
            new LoyaltyReward
            {
                Id = Guid.NewGuid(),
                Name = "50 RON Off",
                Description = "Get 50 RON off your next order - Silver+ Only (min. 70 RON)",
                PointsCost = 500,
                DiscountValue = 50.00m,
                IsActive = true,
                MinimumTier = "Silver",
                MinimumOrderAmount = 70.00m,
                ValidFrom = DateTimeOffset.UtcNow,
                ValidUntil = DateTimeOffset.UtcNow.AddYears(1),
            },
            new LoyaltyReward
            {
                Id = Guid.NewGuid(),
                Name = "20 RON Off",
                Description = "Get 20 RON off your next order (min. 30 RON)",
                PointsCost = 150,
                DiscountValue = 20.00m,
                IsActive = true,
                MinimumTier = "Bronze",
                MinimumOrderAmount = 30.00m,
                ValidFrom = DateTimeOffset.UtcNow,
                ValidUntil = DateTimeOffset.UtcNow.AddYears(1),
            },
            new LoyaltyReward
            {
                Id = Guid.NewGuid(),
                Name = "75 RON Off - Gold Exclusive",
                Description = "Exclusive 75 RON discount for Gold members and above (min. 100 RON)",
                PointsCost = 750,
                DiscountValue = 75.00m,
                IsActive = true,
                MinimumTier = "Gold",
                MinimumOrderAmount = 100.00m,
                ValidFrom = DateTimeOffset.UtcNow,
                ValidUntil = DateTimeOffset.UtcNow.AddYears(1),
            },
            new LoyaltyReward
            {
                Id = Guid.NewGuid(),
                Name = "125 RON Off - Platinum Special",
                Description = "Exclusive 125 RON discount for platinum members (min. 150 RON)",
                PointsCost = 1000,
                DiscountValue = 125.00m,
                IsActive = true,
                MinimumTier = "Platinum",
                MinimumOrderAmount = 150.00m,
                ValidFrom = DateTimeOffset.UtcNow,
                ValidUntil = DateTimeOffset.UtcNow.AddYears(1),
            },
        };

        context.LoyaltyRewards.AddRange(rewards);
        await context.SaveChangesAsync();
    }
}
