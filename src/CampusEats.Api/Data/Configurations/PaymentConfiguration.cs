using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CampusEats.Api.Data.Entities;

namespace CampusEats.Api.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Status).HasMaxLength(64);
        builder.Property(x => x.PaymentMethod).HasMaxLength(128);
        builder.Property(x => x.TransactionId).HasMaxLength(256);
        builder.Property(x => x.CreatedAt);

        builder.HasIndex(x => x.OrderId);
    }
}
