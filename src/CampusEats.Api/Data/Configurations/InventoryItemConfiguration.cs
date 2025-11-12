using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CampusEats.Api.Data.Entities;

namespace CampusEats.Api.Data.Configurations;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("inventory_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(250);
        builder.Property(x => x.Unit).HasMaxLength(64);
        builder.Property(x => x.CurrentQuantity).HasColumnType("decimal(18,2)");
        builder.Property(x => x.MinimumQuantity).HasColumnType("decimal(18,2)");
    }
}
