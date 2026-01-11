using CampusEats.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Data;

public class CampusDbContext : DbContext
{
    public CampusDbContext(DbContextOptions<CampusDbContext> options)
        : base(options) { }

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
    public DbSet<MenuItemAllergen> MenuItemAllergens { get; set; }
    public DbSet<MenuItemDietaryRestriction> MenuItemDietaryRestrictions { get; set; }
    public DbSet<MenuItemIngredient> MenuItemIngredients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Special configuration: Ignore User navigation property - User is managed by IdentityDbContext
        modelBuilder.Entity<Order>(b =>
        {
            b.Ignore(o => o.User);
        });

        // Apply all entity configurations from Configuration classes
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CampusDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        SetTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var entity = entry.Entity;
            var now = DateTimeOffset.UtcNow;

            if (entry.State == EntityState.Added)
            {
                // Set CreatedAt for new entities
                var createdAtProp = entity.GetType().GetProperty("CreatedAt");
                if (createdAtProp != null && createdAtProp.PropertyType == typeof(DateTimeOffset))
                {
                    var currentValue = (DateTimeOffset?)createdAtProp.GetValue(entity);
                    if (currentValue == null || currentValue == default(DateTimeOffset))
                    {
                        createdAtProp.SetValue(entity, now);
                    }
                }
            }

            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                // Set UpdatedAt for new or modified entities
                var updatedAtProp = entity.GetType().GetProperty("UpdatedAt");
                if (updatedAtProp != null && updatedAtProp.PropertyType == typeof(DateTimeOffset))
                {
                    updatedAtProp.SetValue(entity, now);
                }
            }
        }
    }
}
