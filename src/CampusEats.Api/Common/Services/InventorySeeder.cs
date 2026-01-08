using CampusEats.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Common.Services;

public static class InventorySeeder
{
    public static async Task SeedInventory(DbContext context)
    {
        var campusDbContext = context as Data.CampusDbContext;
        if (campusDbContext == null) return;

        // Check if the column exists before trying to seed
        try
        {
            if (await campusDbContext.InventoryItems.AnyAsync())
            {
                return;
            }
        }
        catch
        {
            // Column might not exist yet, skip seeding
            return;
        }

        var inventoryItems = new List<InventoryItem>
        {
            // Baking & Bread
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "All-Purpose Flour",
                Unit = "kg",
                CurrentQuantity = 50m,
                MinimumQuantity = 10m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Bread Rolls",
                Unit = "pieces",
                CurrentQuantity = 200m,
                MinimumQuantity = 50m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Tortilla Wraps",
                Unit = "pieces",
                CurrentQuantity = 150m,
                MinimumQuantity = 30m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            
            // Dairy
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Whole Milk",
                Unit = "L",
                CurrentQuantity = 30m,
                MinimumQuantity = 10m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Cheddar Cheese",
                Unit = "kg",
                CurrentQuantity = 15m,
                MinimumQuantity = 5m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Mozzarella Cheese",
                Unit = "kg",
                CurrentQuantity = 20m,
                MinimumQuantity = 5m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Butter",
                Unit = "kg",
                CurrentQuantity = 10m,
                MinimumQuantity = 3m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Eggs",
                Unit = "dozen",
                CurrentQuantity = 50m,
                MinimumQuantity = 15m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            
            // Proteins
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Chicken Breast",
                Unit = "kg",
                CurrentQuantity = 25m,
                MinimumQuantity = 10m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Ground Beef",
                Unit = "kg",
                CurrentQuantity = 20m,
                MinimumQuantity = 8m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Bacon",
                Unit = "kg",
                CurrentQuantity = 10m,
                MinimumQuantity = 3m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Salmon Fillet",
                Unit = "kg",
                CurrentQuantity = 8m,
                MinimumQuantity = 3m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            
            // Vegetables
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Tomatoes",
                Unit = "kg",
                CurrentQuantity = 30m,
                MinimumQuantity = 10m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Lettuce",
                Unit = "heads",
                CurrentQuantity = 40m,
                MinimumQuantity = 15m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Onions",
                Unit = "kg",
                CurrentQuantity = 20m,
                MinimumQuantity = 5m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Bell Peppers",
                Unit = "kg",
                CurrentQuantity = 15m,
                MinimumQuantity = 5m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Potatoes",
                Unit = "kg",
                CurrentQuantity = 50m,
                MinimumQuantity = 15m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Cucumbers",
                Unit = "pieces",
                CurrentQuantity = 30m,
                MinimumQuantity = 10m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            
            // Condiments & Sauces
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Ketchup",
                Unit = "L",
                CurrentQuantity = 10m,
                MinimumQuantity = 3m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Mayonnaise",
                Unit = "L",
                CurrentQuantity = 8m,
                MinimumQuantity = 3m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Mustard",
                Unit = "L",
                CurrentQuantity = 5m,
                MinimumQuantity = 2m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Olive Oil",
                Unit = "L",
                CurrentQuantity = 15m,
                MinimumQuantity = 5m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            
            // Beverages
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Coffee Beans",
                Unit = "kg",
                CurrentQuantity = 10m,
                MinimumQuantity = 3m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Orange Juice",
                Unit = "L",
                CurrentQuantity = 25m,
                MinimumQuantity = 10m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Cola",
                Unit = "L",
                CurrentQuantity = 40m,
                MinimumQuantity = 15m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            
            // Spices & Seasonings
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Salt",
                Unit = "kg",
                CurrentQuantity = 5m,
                MinimumQuantity = 2m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Black Pepper",
                Unit = "kg",
                CurrentQuantity = 3m,
                MinimumQuantity = 1m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Garlic Powder",
                Unit = "kg",
                CurrentQuantity = 2m,
                MinimumQuantity = 0.5m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Paprika",
                Unit = "kg",
                CurrentQuantity = 2m,
                MinimumQuantity = 0.5m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            
            // Frozen Items
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "French Fries",
                Unit = "kg",
                CurrentQuantity = 40m,
                MinimumQuantity = 15m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Ice Cream (Vanilla)",
                Unit = "L",
                CurrentQuantity = 20m,
                MinimumQuantity = 5m,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        };

        await campusDbContext.InventoryItems.AddRangeAsync(inventoryItems);
        await campusDbContext.SaveChangesAsync();
    }
}
