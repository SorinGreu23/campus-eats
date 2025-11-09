namespace CampusEats.Api.Data.Entities;

public class DietaryRestriction : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    
    public ICollection<MenuItemDietaryRestriction> MenuItemDietaryRestrictions { get; set; } = new List<MenuItemDietaryRestriction>();
}

