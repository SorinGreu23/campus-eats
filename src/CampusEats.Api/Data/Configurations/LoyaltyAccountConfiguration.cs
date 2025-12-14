using CampusEats.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampusEats.Api.Data.Configurations;

public class LoyaltyAccountConfiguration : IEntityTypeConfiguration<LoyaltyAccount>
{
    public void Configure(EntityTypeBuilder<LoyaltyAccount> builder)
    {
        builder.ToTable("loyalty_accounts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PointsBalance).HasDefaultValue(0);
        builder.Property(x => x.LifetimePoints).HasDefaultValue(0);
        builder.Property(x => x.Tier).HasMaxLength(64);
    }
}
