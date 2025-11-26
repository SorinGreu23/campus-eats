using CampusEats.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Data;

public class CampusDbContext : DbContext
{
    public CampusDbContext(DbContextOptions<CampusDbContext> options) : base(options)
    {
    }

    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
    public DbSet<LoyaltyAccount> LoyaltyAccounts { get; set; }
    public DbSet<LoyaltyReward> LoyaltyRewards { get; set; }
    public DbSet<LoyaltyClaim> LoyaltyClaims { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Allergen> Allergens { get; set; }
    public DbSet<DietaryRestriction> DietaryRestrictions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Suppress pending model changes warning as we've cleaned up duplicate configurations
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Special configuration: Ignore User navigation property - User is managed by IdentityDbContext
        builder.Entity<Order>(b =>
        {
            b.Ignore(o => o.User);
        });

        // Apply all entity configurations from Configuration classes
        builder.ApplyConfigurationsFromAssembly(typeof(CampusDbContext).Assembly);
    }
}