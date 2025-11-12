using System;

namespace CampusEats.Api.Data.Entities;

public class LoyaltyReward
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int PointsCost { get; set; }
    public decimal? DiscountValue { get; set; }
    public Guid? MenuItemId { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset? ValidFrom { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
}
