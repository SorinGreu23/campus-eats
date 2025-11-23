using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CampusEats.Api.Data.Entities;
namespace CampusEats.Api.Data.Configurations;
public class DietaryRestrictionConfiguration : IEntityTypeConfiguration<DietaryRestriction>
{
    public void Configure(EntityTypeBuilder<DietaryRestriction> builder)
    {
        builder.ToTable("dietary_restrictions");
        builder.HasKey(dr => dr.Id);
        builder.Property(dr => dr.Id).ValueGeneratedNever();
        builder.Property(dr => dr.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(dr => dr.Icon)
            .HasMaxLength(50);
    }
}
