namespace CampusEats.Api.Data.Entities;
public class DietaryRestriction
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Icon { get; set; }
    public ICollection<MenuItemDietaryRestriction> MenuItemDietaryRestrictions { get; set; } = new List<MenuItemDietaryRestriction>();
}
