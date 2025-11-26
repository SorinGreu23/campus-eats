using System;

namespace CampusEats.Api.Data.Entities;

public class MenuItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public Guid? CategoryId { get; set; }
    public string? ImageUrl { get; set; }
    public int? PreparationTimeMinutes { get; set; }
    public bool IsAvailable { get; set; }
    public int? Calories { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Category? Category { get; set; }
    public ICollection<MenuItemAllergen> MenuItemAllergens { get; set; } = new List<MenuItemAllergen>();
    public ICollection<MenuItemDietaryRestriction> MenuItemDietaryRestrictions { get; set; } = new List<MenuItemDietaryRestriction>();
}
