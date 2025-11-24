using System;

namespace CampusEats.Api.Data.Entities;

public class MenuItemIngredient
{
    public Guid Id { get; set; }
    public Guid MenuItemId { get; set; }
    public Guid InventoryItemId { get; set; }
    public decimal QuantityRequired { get; set; }

    public MenuItem MenuItem { get; set; } = null!;
    public InventoryItem InventoryItem { get; set; } = null!;
}
