using System;

namespace CampusEats.Api.Data.Entities;

public class InventoryItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Unit { get; set; } = null!;
    public decimal CurrentQuantity { get; set; }
    public decimal MinimumQuantity { get; set; }    public bool IsOutOfStock { get; set; }    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();
}
