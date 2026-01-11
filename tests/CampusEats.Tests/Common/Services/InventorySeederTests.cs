using System;
using System.Linq;
using System.Threading.Tasks;
using CampusEats.Api.Common.Services;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Tests.Common.Services;

public class InventorySeederTests
{
    private static CampusDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CampusDbContext(options);
    }

    private class FakeDbContext : DbContext
    {
        public FakeDbContext(DbContextOptions options) : base(options) { }
    }

    [Fact]
    public async Task SeedInventory_WhenEmpty_AddsInitialItems()
    {
        await using var db = CreateDbContext();
        (await db.InventoryItems.CountAsync()).Should().Be(0);

        await InventorySeeder.SeedInventory(db);

        var count = await db.InventoryItems.CountAsync();
        count.Should().BeGreaterThanOrEqualTo(25);

        var names = await db.InventoryItems.Select(i => i.Name).ToListAsync();
        names.Should().Contain(new[]
        {
            "All-Purpose Flour",
            "Whole Milk",
            "Chicken Breast",
            "Tomatoes",
            "Salt",
            "French Fries"
        });
    }

    [Fact]
    public async Task SeedInventory_WhenAlreadySeeded_DoesNothing()
    {
        await using var db = CreateDbContext();
        db.InventoryItems.Add(new InventoryItem
        {
            Id = Guid.NewGuid(),
            Name = "Test Item",
            Unit = "pcs",
            CurrentQuantity = 1,
            MinimumQuantity = 0,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        await InventorySeeder.SeedInventory(db);

        var count = await db.InventoryItems.CountAsync();
        count.Should().Be(1);
        var names = await db.InventoryItems.Select(i => i.Name).ToListAsync();
        names.Should().Contain("Test Item");
        names.Should().NotContain("All-Purpose Flour");
    }

    [Fact]
    public async Task SeedInventory_WithNonCampusDbContext_ReturnsEarly()
    {
        var options = new DbContextOptionsBuilder().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var fake = new FakeDbContext(options);

        Func<Task> act = async () => await InventorySeeder.SeedInventory(fake);
        await act.Should().NotThrowAsync();
    }
}
