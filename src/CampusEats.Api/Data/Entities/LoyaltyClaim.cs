using System;

namespace CampusEats.Api.Data.Entities;

public class LoyaltyClaim
{
    public Guid Id { get; set; }
    public Guid LoyaltyAccountId { get; set; }
    public Guid RewardId { get; set; }
    public DateTimeOffset ClaimedAt { get; set; }
    public string? Notes { get; set; }

    // navigation
    public LoyaltyAccount? LoyaltyAccount { get; set; }
    public LoyaltyReward? LoyaltyReward { get; set; }
}
