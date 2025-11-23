using System;

namespace CampusEats.Api.Data.Entities;

public class LoyaltyAccount
{
    public Guid Id { get; set; }
    public string? UserId { get; set; }
    public int PointsBalance { get; set; }
    public int LifetimePoints { get; set; }
    public string? Tier { get; set; }
}
