namespace CampusEats.Api.Data.Entities;

public class MenuItemDietaryRestriction
{
    public Guid MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;
    public Guid DietaryRestrictionId { get; set; }
    public DietaryRestriction DietaryRestriction { get; set; } = null!;
}
