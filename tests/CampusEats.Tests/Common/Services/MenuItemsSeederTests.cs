using System;
using System.Linq;
using System.Threading.Tasks;
using CampusEats.Api.Common.Services;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Tests.Common.Services;

public class MenuItemsSeederTests
{
    private static CampusDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CampusDbContext(options);
    }

    [Fact]
    public async Task SeedMenuItems_WhenEmptyDb_CreatesItemsAndAssociations()
    {
        await using var db = CreateDbContext();
        (await db.MenuItems.CountAsync()).Should().Be(0);

        await MenuItemsSeeder.SeedMenuItems(db);

        var items = await db.MenuItems.ToListAsync();
        items.Count.Should().Be(5);

        var idsByName = items.ToDictionary(i => i.Name, i => i.Id);
        int Allergens(string name) => db.MenuItemAllergens.Count(a => a.MenuItemId == idsByName[name]);
        int Restrictions(string name) => db.MenuItemDietaryRestrictions.Count(r => r.MenuItemId == idsByName[name]);

        Allergens("Classic Cheeseburger").Should().Be(4);
        Restrictions("Classic Cheeseburger").Should().Be(1);

        Allergens("Grilled Chicken Wrap").Should().Be(2);
        Restrictions("Grilled Chicken Wrap").Should().Be(1);

        Allergens("Veggie Salad Bowl").Should().Be(1);
        Restrictions("Veggie Salad Bowl").Should().Be(4);

        Allergens("Spicy Ramen").Should().Be(3);
        Restrictions("Spicy Ramen").Should().Be(1);

        Allergens("Chocolate Brownie").Should().Be(3);
        Restrictions("Chocolate Brownie").Should().Be(1);
    }

    [Fact]
    public async Task SeedMenuItems_WhenExistingItemsWithoutAssociations_AddsAssociationsOnly()
    {
        await using var db = CreateDbContext();

        db.MenuItems.Add(new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = "Veggie Salad Bowl",
            Price = 6.5m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var beforeItems = await db.MenuItems.CountAsync();
        var beforeAllergens = await db.MenuItemAllergens.CountAsync();
        var beforeRestrictions = await db.MenuItemDietaryRestrictions.CountAsync();

        await MenuItemsSeeder.SeedMenuItems(db);

        var afterItems = await db.MenuItems.CountAsync();
        var afterAllergens = await db.MenuItemAllergens.CountAsync();
        var afterRestrictions = await db.MenuItemDietaryRestrictions.CountAsync();

        afterItems.Should().Be(beforeItems); // No new items created

        var itemId = await db.MenuItems.Where(i => i.Name == "Veggie Salad Bowl").Select(i => i.Id).FirstAsync();
        db.MenuItemAllergens.Count(a => a.MenuItemId == itemId).Should().Be(1); // Sesame
        db.MenuItemDietaryRestrictions.Count(r => r.MenuItemId == itemId).Should().Be(4); // Vegetarian, Vegan, Gluten-Free, Lactose-Free

        afterAllergens.Should().BeGreaterThan(beforeAllergens);
        afterRestrictions.Should().BeGreaterThan(beforeRestrictions);
    }

    [Fact]
    public async Task SeedMenuItems_WhenAssociationsExist_DoesNothing()
    {
        await using var db = CreateDbContext();

        var item = new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = "Chocolate Brownie",
            Price = 3.75m,
            IsAvailable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.MenuItems.Add(item);
        db.MenuItemAllergens.Add(new MenuItemAllergen
        {
            MenuItemId = item.Id,
            AllergenId = Guid.NewGuid() // any id, presence is enough
        });
        await db.SaveChangesAsync();

        var beforeItems = await db.MenuItems.CountAsync();
        var beforeAllergens = await db.MenuItemAllergens.CountAsync();
        var beforeRestrictions = await db.MenuItemDietaryRestrictions.CountAsync();

        await MenuItemsSeeder.SeedMenuItems(db);

        var afterItems = await db.MenuItems.CountAsync();
        var afterAllergens = await db.MenuItemAllergens.CountAsync();
        var afterRestrictions = await db.MenuItemDietaryRestrictions.CountAsync();

        afterItems.Should().Be(beforeItems);
        afterAllergens.Should().Be(beforeAllergens);
        afterRestrictions.Should().Be(beforeRestrictions);
    }
}
