using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CampusEats.Api.Data.Entities;
namespace CampusEats.Api.Data.Configurations;
public class MenuItemAllergenConfiguration : IEntityTypeConfiguration<MenuItemAllergen>
{
    public void Configure(EntityTypeBuilder<MenuItemAllergen> builder)
    {
        builder.ToTable("menu_item_allergens");
        builder.HasKey(mia => new { mia.MenuItemId, mia.AllergenId });
        builder.HasOne(mia => mia.MenuItem)
            .WithMany(m => m.MenuItemAllergens)
            .HasForeignKey(mia => mia.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(mia => mia.Allergen)
            .WithMany(a => a.MenuItemAllergens)
            .HasForeignKey(mia => mia.AllergenId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
