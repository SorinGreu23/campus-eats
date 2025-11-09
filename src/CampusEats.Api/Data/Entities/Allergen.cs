namespace CampusEats.Api.Data.Entities;

public class Allergen : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    
    public ICollection<MenuItemAllergen> MenuItemAllergens { get; set; } = new List<MenuItemAllergen>();
}

