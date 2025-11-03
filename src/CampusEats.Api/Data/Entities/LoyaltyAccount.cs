using System;

namespace CampusEats.Api.Data.Entities;

public class LoyaltyAccount : BaseEntity
{
    public Guid? UserId { get; set; }
    public int PointsBalance { get; set; }
    public int LifetimePoints { get; set; }
    public string? Tier { get; set; }
}
