using CampusEats.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampusEats.Api.Data.Configurations;

public class LoyaltyClaimConfiguration : IEntityTypeConfiguration<LoyaltyClaim>
{
    public void Configure(EntityTypeBuilder<LoyaltyClaim> builder)
    {
        builder.ToTable("LoyaltyClaim");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.LoyaltyAccountId).IsRequired();
        builder.Property(x => x.RewardId).IsRequired();
        builder.Property(x => x.ClaimedAt).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(500);

        builder.HasOne(x => x.LoyaltyAccount)
            .WithMany()
            .HasForeignKey(x => x.LoyaltyAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.LoyaltyReward)
            .WithMany()
            .HasForeignKey(x => x.RewardId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
