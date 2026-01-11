using System;

namespace CampusEats.Api.Data.Entities;

/// <summary>
/// Represents the relationship between menu items and inventory items (ingredients/recipe).
/// Tracks how much of each ingredient is required to prepare one unit of a menu item.
/// </summary>
public class MenuItemIngredient
{
    public Guid MenuItemId { get; set; }
    public Guid InventoryItemId { get; set; }
    
    /// <summary>
    /// The quantity of the inventory item required per single menu item.
    /// For example, if a burger requires 0.25 kg of beef, this would be 0.25.
    /// </summary>
    public decimal QuantityRequired { get; set; }

    // Navigation properties
    public MenuItem MenuItem { get; set; } = null!;
    public InventoryItem InventoryItem { get; set; } = null!;
}
