namespace CampusEats.Api.Data.Entities;

/// <summary>
/// Represents substances that may cause allergic reactions (e.g., Nuts, Dairy, Gluten, Shellfish, Soy)
/// </summary>
public class Allergen
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Icon { get; set; }

    public ICollection<MenuItemAllergen> MenuItemAllergens { get; set; } = new List<MenuItemAllergen>();
}


