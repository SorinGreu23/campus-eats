using System;

namespace CampusEats.Api.Data.Entities;

public class InventoryItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Unit { get; set; }
    public decimal CurrentQuantity { get; set; }
    public decimal MinimumQuantity { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
