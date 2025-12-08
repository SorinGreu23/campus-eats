using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CampusEats.Api.Data.Entities;
namespace CampusEats.Api.Data.Configurations;
public class MenuItemDietaryRestrictionConfiguration : IEntityTypeConfiguration<MenuItemDietaryRestriction>
{
    public void Configure(EntityTypeBuilder<MenuItemDietaryRestriction> builder)
    {
        builder.ToTable("menu_item_dietary_restrictions");
        builder.HasKey(midr => new { midr.MenuItemId, midr.DietaryRestrictionId });
        builder.HasOne(midr => midr.MenuItem)
            .WithMany(m => m.MenuItemDietaryRestrictions)
            .HasForeignKey(midr => midr.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(midr => midr.DietaryRestriction)
            .WithMany(dr => dr.MenuItemDietaryRestrictions)
            .HasForeignKey(midr => midr.DietaryRestrictionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
