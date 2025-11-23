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
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<MenuItem>(b =>
        {
            b.ToTable("MenuItem");
            b.Property(m => m.Id).ValueGeneratedNever();
            b.Property(m => m.Name).IsRequired();
            b.Property(m => m.Description).IsRequired();
            b.Property(m => m.Price).IsRequired();
            b.Property(m => m.CategoryId).IsRequired();
        });

        builder.Entity<Category>(b =>
        {
            b.ToTable("Category");
            b.Property(c => c.Id).ValueGeneratedNever();
            b.Property(c => c.Name).IsRequired();
        });

        builder.Entity<Order>(b =>
        {
            b.ToTable("Order");
            b.Property(o => o.Id).ValueGeneratedNever();
            b.Property(o => o.UserId).IsRequired();
            b.Property(o => o.Status).IsRequired();
            
            // Ignore User navigation property - User is managed by IdentityDbContext
            b.Ignore(o => o.User);
        });

        builder.Entity<OrderItem>(b =>
        {
            b.ToTable("OrderItem");
            b.Property(o => o.Id).ValueGeneratedNever();
            b.Property(o => o.OrderId).IsRequired();
            b.Property(o => o.MenuItemId).IsRequired();
            b.Property(o => o.Quantity).IsRequired();
        });

        builder.Entity<Payment>(b =>
        {
            b.ToTable("Payment");
            b.Property(p => p.Id).ValueGeneratedNever();
            b.Property(p => p.OrderId).IsRequired();
            b.Property(p => p.Amount).IsRequired();
            b.Property(p => p.PaymentMethod).IsRequired();
        });

        builder.Entity<InventoryItem>(b =>
        {
            b.ToTable("InventoryItem");
            b.Property(i => i.Id).ValueGeneratedNever();
            b.Property(i => i.Name).IsRequired();
        });

        builder.Entity<InventoryTransaction>(b =>
        {
            b.ToTable("InventoryTransaction");
            b.Property(i => i.Id).ValueGeneratedNever();
            b.Property(i => i.InventoryItemId).IsRequired();
            b.Property(i => i.Quantity).IsRequired();
            b.Property(i => i.TransactionType).IsRequired();
        });

        builder.Entity<LoyaltyAccount>(b =>
        {
            b.ToTable("LoyaltyAccount");
            b.Property(l => l.Id).ValueGeneratedNever();
            b.Property(l => l.UserId).IsRequired();

        });

        builder.Entity<LoyaltyReward>(b =>
        {
            b.ToTable("LoyaltyReward");
            b.Property(l => l.Id).ValueGeneratedNever();
        });

        builder.Entity<Notification>(b =>
        {
            b.ToTable("Notification");
            b.Property(n => n.Id).ValueGeneratedNever();
            b.Property(n => n.UserId).IsRequired();
            b.Property(n => n.Message).IsRequired();
        });
        
        builder.ApplyConfigurationsFromAssembly(typeof(CampusDbContext).Assembly);
    }
}