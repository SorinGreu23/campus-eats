using System;

namespace CampusEats.Api.Data.Entities;

public class InventoryItem : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Unit { get; set; }
    public decimal CurrentQuantity { get; set; }
    public decimal MinimumQuantity { get; set; }
}
