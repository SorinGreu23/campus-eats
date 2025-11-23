namespace CampusEats.Api.Data.Entities;

public class Allergen
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Icon { get; set; }

    public ICollection<MenuItemAllergen> MenuItemAllergens { get; set; } = new List<MenuItemAllergen>();
}


