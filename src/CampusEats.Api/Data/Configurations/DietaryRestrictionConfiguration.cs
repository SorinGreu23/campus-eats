using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CampusEats.Api.Data.Entities;

namespace CampusEats.Api.Data.Configurations;

public class DietaryRestrictionConfiguration : IEntityTypeConfiguration<DietaryRestriction>
{
    public void Configure(EntityTypeBuilder<DietaryRestriction> builder)
    {
        builder.ToTable("dietary_restrictions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.CreatedAt);
        builder.Property(x => x.UpdatedAt);
    }
}

