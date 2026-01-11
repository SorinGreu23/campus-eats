using CampusEats.Api.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.LoyaltyPoints.RedeemReward;

public class RedeemRewardHandler : IRequestHandler<RedeemRewardRequest, IResult>
{
    private readonly CampusDbContext _context;

    public RedeemRewardHandler(CampusDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(
        RedeemRewardRequest request,
        CancellationToken cancellationToken
    )
    {
        // Get loyalty account
        var loyaltyAccount = await _context.LoyaltyAccounts.FirstOrDefaultAsync(
            la => la.UserId == request.UserId,
            cancellationToken
        );

        if (loyaltyAccount == null)
        {
            return Results.NotFound("Loyalty account not found.");
        }

        // Get reward
        var reward = await _context.LoyaltyRewards.FirstOrDefaultAsync(
            r => r.Id == request.RewardId,
            cancellationToken
        );

        if (reward == null)
        {
            return Results.NotFound("Reward not found.");
        }

        // Check if reward is active
        if (!reward.IsActive)
        {
            return Results.BadRequest("This reward is not currently available.");
        }

        // Check tier requirement
        if (!string.IsNullOrEmpty(reward.MinimumTier))
        {
            var userTierRank = GetTierRank(loyaltyAccount.Tier ?? "Bronze");
            var requiredTierRank = GetTierRank(reward.MinimumTier);

            if (userTierRank < requiredTierRank)
            {
                return Results.BadRequest(
                    $"This reward requires {reward.MinimumTier} tier or higher. Your current tier is {loyaltyAccount.Tier ?? "Bronze"}."
                );
            }
        }

        // Check validity dates
        var now = DateTimeOffset.UtcNow;
        if (reward.ValidFrom.HasValue && reward.ValidFrom > now)
        {
            return Results.BadRequest("This reward is not yet valid.");
        }

        if (reward.ValidUntil.HasValue && reward.ValidUntil < now)
        {
            return Results.BadRequest("This reward has expired.");
        }

        // Check if user has enough points
        if (loyaltyAccount.PointsBalance < reward.PointsCost)
        {
            return Results.BadRequest(
                $"Insufficient points. You need {reward.PointsCost} points but only have {loyaltyAccount.PointsBalance}."
            );
        }

        // Check if user already has an unused claim for this reward
        var existingClaim = await _context.LoyaltyClaims.FirstOrDefaultAsync(
            c =>
                c.LoyaltyAccountId == loyaltyAccount.Id
                && c.RewardId == reward.Id
                && c.Notes != "Used",
            cancellationToken
        );

        if (existingClaim != null)
        {
            return Results.BadRequest("You have already claimed this reward.");
        }

        // DO NOT deduct points yet - points will be deducted when the order is placed
        // This prevents losing points if the user doesn't complete the order

        // Create claim record (reservation)
        var claim = new CampusEats.Api.Data.Entities.LoyaltyClaim
        {
            Id = Guid.NewGuid(),
            LoyaltyAccountId = loyaltyAccount.Id,
            RewardId = reward.Id,
            ClaimedAt = DateTimeOffset.UtcNow,
            Notes = request.Reason ?? "Claimed for future use",
        };

        // Save claim
        _context.LoyaltyClaims.Add(claim);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new RedeemRewardResponse
        {
            AccountId = loyaltyAccount.Id,
            NewPointsBalance = loyaltyAccount.PointsBalance,
            Message = $"Successfully redeemed {reward.Name}!",
            ClaimId = claim.Id,
        };

        return Results.Ok(response);
    }

    private static int GetTierRank(string tier)
    {
        return tier switch
        {
            "Bronze" => 1,
            "Silver" => 2,
            "Gold" => 3,
            "Platinum" => 4,
            _ => 0,
        };
    }
}
