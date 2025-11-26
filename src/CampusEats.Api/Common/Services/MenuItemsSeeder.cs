using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;

namespace CampusEats.Api.Common.Services;

public static class MenuItemsSeeder
{
    public static async Task SeedMenuItems(CampusDbContext context)
    {
        if (await context.MenuItems.AnyAsync())
        {
            return; // Already seeded
        }

        // Ensure categories exist; if none, create defaults
        if (!await context.Categories.AnyAsync())
        {
            await CategoriesSeeder.SeedCategories(context);
        }

        var categories = await context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
        var burgersCat = categories.FirstOrDefault(c => c.Name == "Burgers")?.Id;
        var wrapsCat = categories.FirstOrDefault(c => c.Name == "Wraps")?.Id;
        var saladsCat = categories.FirstOrDefault(c => c.Name == "Salads")?.Id;
        var noodlesCat = categories.FirstOrDefault(c => c.Name == "Noodles")?.Id;
        var dessertsCat = categories.FirstOrDefault(c => c.Name == "Desserts")?.Id;

        var now = DateTimeOffset.UtcNow;

        var items = new List<MenuItem>
        {
            new MenuItem
            {
                Id = Guid.NewGuid(),
                Name = "Classic Cheeseburger",
                Description = "Juicy beef patty with cheddar, lettuce, tomato, and our special sauce.",
                Price = 8.99m,
                CategoryId = burgersCat,
                ImageUrl = null,
                PreparationTimeMinutes = 15,
                IsAvailable = true,
                Calories = 750,
                CreatedAt = now,
                UpdatedAt = now
            },
            new MenuItem
            {
                Id = Guid.NewGuid(),
                Name = "Grilled Chicken Wrap",
                Description = "Marinated grilled chicken with fresh greens and a light vinaigrette.",
                Price = 7.49m,
                CategoryId = wrapsCat,
                ImageUrl = null,
                PreparationTimeMinutes = 10,
                IsAvailable = true,
                Calories = 420,
                CreatedAt = now,
                UpdatedAt = now
            },
            new MenuItem
            {
                Id = Guid.NewGuid(),
                Name = "Veggie Salad Bowl",
                Description = "Seasonal vegetables, quinoa, and a lemon-tahini dressing.",
                Price = 6.50m,
                CategoryId = saladsCat,
                ImageUrl = null,
                PreparationTimeMinutes = 8,
                IsAvailable = true,
                Calories = 320,
                CreatedAt = now,
                UpdatedAt = now
            },
            new MenuItem
            {
                Id = Guid.NewGuid(),
                Name = "Spicy Ramen",
                Description = "Hot and savory broth with noodles, egg, and spicy chili oil.",
                Price = 9.25m,
                CategoryId = noodlesCat,
                ImageUrl = null,
                PreparationTimeMinutes = 12,
                IsAvailable = true,
                Calories = 610,
                CreatedAt = now,
                UpdatedAt = now
            },
            new MenuItem
            {
                Id = Guid.NewGuid(),
                Name = "Chocolate Brownie",
                Description = "Warm chocolate brownie served with vanilla ice cream.",
                Price = 3.75m,
                CategoryId = dessertsCat,
                ImageUrl = null,
                PreparationTimeMinutes = 5,
                IsAvailable = true,
                Calories = 450,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        context.MenuItems.AddRange(items);
        await context.SaveChangesAsync();
    }
}
