using Microsoft.EntityFrameworkCore;
using CampusEats.Api.Data.Entities;

namespace CampusEats.Api.Data;

public class CampusDbContext : DbContext
{
    public CampusDbContext(DbContextOptions<CampusDbContext> options) : base(options)
    {
    }

    // DbSets for core entities. Add more as you implement them.
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<MenuItem> MenuItems { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<InventoryItem> InventoryItems { get; set; } = null!;
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<LoyaltyAccount> LoyaltyAccounts { get; set; } = null!;
    public DbSet<LoyaltyReward> LoyaltyRewards { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(ToSnakeCase(entity.GetTableName()));
            
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.GetColumnName()));
            }
            
            foreach (var key in entity.GetKeys())
            {
                key.SetName(ToSnakeCase(key.GetName()));
            }
            
            foreach (var fk in entity.GetForeignKeys())
            {
                fk.SetConstraintName(ToSnakeCase(fk.GetConstraintName()));
            }
            
            foreach (var index in entity.GetIndexes())
            {
                index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName()));
            }
        }
        
        modelBuilder.ApplyConfigurationsFromAssembly((typeof(CampusDbContext).Assembly));
    }
    
    private static string? ToSnakeCase(string? name)
    {
        if (string.IsNullOrEmpty(name))
            return name;
            
        return string.Concat(
            name.Select((c, i) => i > 0 && char.IsUpper(c) 
                ? "_" + c.ToString() 
                : c.ToString())
        ).ToLower();
    }
}