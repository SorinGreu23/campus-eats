using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CampusEats.Api.Data.Entities;

namespace CampusEats.Api.Data.Configurations;

public class MenuItemAllergenConfiguration : IEntityTypeConfiguration<MenuItemAllergen>
{
    public void Configure(EntityTypeBuilder<MenuItemAllergen> builder)
    {
        builder.ToTable("menu_item_allergens");
        builder.HasKey(x => new { x.MenuItemId, x.AllergenId });
        
        builder.HasOne(x => x.MenuItem)
            .WithMany(m => m.MenuItemAllergens)
            .HasForeignKey(x => x.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(x => x.Allergen)
            .WithMany(a => a.MenuItemAllergens)
            .HasForeignKey(x => x.AllergenId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

