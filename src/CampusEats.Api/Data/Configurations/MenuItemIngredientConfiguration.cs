using CampusEats.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampusEats.Api.Data.Configurations;

public class MenuItemIngredientConfiguration : IEntityTypeConfiguration<MenuItemIngredient>
{
    public void Configure(EntityTypeBuilder<MenuItemIngredient> builder)
    {
        builder.ToTable("menu_item_ingredients");
        
        // Composite primary key
        builder.HasKey(x => new { x.MenuItemId, x.InventoryItemId });
        
        builder.Property(x => x.QuantityRequired)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        // Relationship to InventoryItem
        builder
            .HasOne(x => x.InventoryItem)
            .WithMany()
            .HasForeignKey(x => x.InventoryItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
