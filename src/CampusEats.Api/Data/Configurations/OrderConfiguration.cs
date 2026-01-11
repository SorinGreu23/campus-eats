using CampusEats.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampusEats.Api.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
  private const string DecimalPrecisionFormat = "decimal(18,2)";

  public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OrderNumber).HasMaxLength(100);
        builder.Property(x => x.Status).HasMaxLength(64);
        builder.Property(x => x.OrderType).HasColumnName("order_type").HasMaxLength(32);
        builder.Property(x => x.Subtotal).HasColumnType(DecimalPrecisionFormat);
        builder.Property(x => x.Tax).HasColumnType(DecimalPrecisionFormat);
        builder.Property(x => x.Discount).HasColumnType(DecimalPrecisionFormat);
        builder.Property(x => x.Total).HasColumnType(DecimalPrecisionFormat);
        builder.Property(x => x.DeliveryInstructions);
        builder.Property(x => x.PickupTime);
        builder.Property(x => x.CompletedAt);
        builder.Property(x => x.CancelledAt);
        builder.Property(x => x.CancellationReason);

        // Timestamps are not persisted in current schema; ignore to prevent missing-column errors
        builder.Ignore(x => x.CreatedAt);
        builder.Ignore(x => x.UpdatedAt);

        // User navigation property is ignored - User is managed by IdentityDbContext
        builder.Ignore(x => x.User);
    }
}
