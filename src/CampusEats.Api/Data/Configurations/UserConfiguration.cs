using CampusEats.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampusEats.Api.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User");
        
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);
        
        builder.Property(u => u.FirstName)
            .HasMaxLength(128);
        
        builder.Property(u => u.LastName)
            .HasMaxLength(128);

        builder.Property(u => u.Role)
            .HasMaxLength(64);

        builder.HasIndex(u => u.Email).IsUnique();
    }
}