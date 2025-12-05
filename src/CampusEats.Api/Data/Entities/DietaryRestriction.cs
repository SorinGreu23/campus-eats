namespace CampusEats.Api.Data.Entities;

/// <summary>
/// Represents dietary preferences or restrictions (e.g., Vegetarian, Vegan, Carnivore, Keto, Halal, Kosher)
/// </summary>
public class DietaryRestriction
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public ICollection<MenuItemDietaryRestriction> MenuItemDietaryRestrictions { get; set; } = new List<MenuItemDietaryRestriction>();
}
