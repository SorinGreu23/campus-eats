using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CampusEats.Api.Data.Entities;

namespace CampusEats.Api.Data.Configurations;

public class LoyaltyRewardConfiguration : IEntityTypeConfiguration<LoyaltyReward>
{
    public void Configure(EntityTypeBuilder<LoyaltyReward> builder)
    {
        builder.ToTable("loyalty_rewards");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description);
        builder.Property(x => x.PointsCost);
        builder.Property(x => x.DiscountValue).HasColumnType("decimal(18,2)");
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.ValidFrom);
        builder.Property(x => x.ValidUntil);
        builder.Property(x => x.MinimumTier).HasMaxLength(50);
    }
}
