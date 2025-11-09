using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CampusEats.Api.Data.Entities;

namespace CampusEats.Api.Data.Configurations;

public class MenuItemDietaryRestrictionConfiguration : IEntityTypeConfiguration<MenuItemDietaryRestriction>
{
    public void Configure(EntityTypeBuilder<MenuItemDietaryRestriction> builder)
    {
        builder.ToTable("menu_item_dietary_restrictions");
        builder.HasKey(x => new { x.MenuItemId, x.DietaryRestrictionId });
        
        builder.HasOne(x => x.MenuItem)
            .WithMany(m => m.MenuItemDietaryRestrictions)
            .HasForeignKey(x => x.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(x => x.DietaryRestriction)
            .WithMany(d => d.MenuItemDietaryRestrictions)
            .HasForeignKey(x => x.DietaryRestrictionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

