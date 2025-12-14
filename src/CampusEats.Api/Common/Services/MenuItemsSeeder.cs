using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Common.Services;

public static class MenuItemsSeeder
{
    public static async Task SeedMenuItems(CampusDbContext context)
    {
        // Ensure categories exist; if none, create defaults
        if (!await context.Categories.AnyAsync())
        {
            await CategoriesSeeder.SeedCategories(context);
        }

        // Ensure allergens and dietary restrictions exist
        if (!await context.Allergens.AnyAsync() || !await context.DietaryRestrictions.AnyAsync())
        {
            await AllergensAndDietaryRestrictionsSeeder.SeedAllergensAndDietaryRestrictions(
                context
            );
        }

        // Check if menu items with associations already exist
        var existingItemsWithAssociations = await context
            .MenuItems.Include(m => m.MenuItemAllergens)
            .Include(m => m.MenuItemDietaryRestrictions)
            .AnyAsync(m => m.MenuItemAllergens.Any() || m.MenuItemDietaryRestrictions.Any());

        if (existingItemsWithAssociations)
        {
            return; // Already seeded with associations
        }

        // Check if menu items exist but without associations
        var existingItems = await context.MenuItems.ToListAsync();
        if (existingItems.Any())
        {
            // Update existing items with associations
            await AddAssociationsToExistingItems(context, existingItems);
            return;
        }

        var categories = await context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
        var burgersCat = categories.FirstOrDefault(c => c.Name == "Burgers")?.Id;
        var wrapsCat = categories.FirstOrDefault(c => c.Name == "Wraps")?.Id;
        var saladsCat = categories.FirstOrDefault(c => c.Name == "Salads")?.Id;
        var noodlesCat = categories.FirstOrDefault(c => c.Name == "Noodles")?.Id;
        var dessertsCat = categories.FirstOrDefault(c => c.Name == "Desserts")?.Id;

        // Load allergens
        var allergens = await context.Allergens.ToListAsync();
        var milkAllergen = allergens.FirstOrDefault(a => a.Name == "Milk/Dairy");
        var wheatAllergen = allergens.FirstOrDefault(a => a.Name == "Wheat/Gluten");
        var eggsAllergen = allergens.FirstOrDefault(a => a.Name == "Eggs");
        var soyAllergen = allergens.FirstOrDefault(a => a.Name == "Soy");
        var sesameAllergen = allergens.FirstOrDefault(a => a.Name == "Sesame");

        // Load dietary restrictions
        var dietaryRestrictions = await context.DietaryRestrictions.ToListAsync();
        var vegetarian = dietaryRestrictions.FirstOrDefault(d => d.Name == "Vegetarian");
        var vegan = dietaryRestrictions.FirstOrDefault(d => d.Name == "Vegan");
        var carnivore = dietaryRestrictions.FirstOrDefault(d => d.Name == "Carnivore");
        var glutenFree = dietaryRestrictions.FirstOrDefault(d => d.Name == "Gluten-Free");
        var lactoseFree = dietaryRestrictions.FirstOrDefault(d => d.Name == "Lactose-Free");

        var now = DateTimeOffset.UtcNow;

        // Create menu items
        var classicCheeseburger = new MenuItem
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
            UpdatedAt = now,
        };

        var grilledChickenWrap = new MenuItem
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
            UpdatedAt = now,
        };

        var veggieSaladBowl = new MenuItem
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
            UpdatedAt = now,
        };

        var spicyRamen = new MenuItem
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
            UpdatedAt = now,
        };

        var chocolateBrownie = new MenuItem
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
            UpdatedAt = now,
        };

        var items = new List<MenuItem>
        {
            classicCheeseburger,
            grilledChickenWrap,
            veggieSaladBowl,
            spicyRamen,
            chocolateBrownie,
        };

        context.MenuItems.AddRange(items);
        await context.SaveChangesAsync();

        // Add allergen associations
        var menuItemAllergens = new List<MenuItemAllergen>();

        // Classic Cheeseburger: Milk/Dairy, Wheat/Gluten, Eggs, Soy
        if (milkAllergen != null)
            menuItemAllergens.Add(
                new MenuItemAllergen
                {
                    MenuItemId = classicCheeseburger.Id,
                    AllergenId = milkAllergen.Id,
                }
            );
        if (wheatAllergen != null)
            menuItemAllergens.Add(
                new MenuItemAllergen
                {
                    MenuItemId = classicCheeseburger.Id,
                    AllergenId = wheatAllergen.Id,
                }
            );
        if (eggsAllergen != null)
            menuItemAllergens.Add(
                new MenuItemAllergen
                {
                    MenuItemId = classicCheeseburger.Id,
                    AllergenId = eggsAllergen.Id,
                }
            );
        if (soyAllergen != null)
            menuItemAllergens.Add(
                new MenuItemAllergen
                {
                    MenuItemId = classicCheeseburger.Id,
                    AllergenId = soyAllergen.Id,
                }
            );

        // Grilled Chicken Wrap: Wheat/Gluten, Soy
        if (wheatAllergen != null)
            menuItemAllergens.Add(
                new MenuItemAllergen
                {
                    MenuItemId = grilledChickenWrap.Id,
                    AllergenId = wheatAllergen.Id,
                }
            );
        if (soyAllergen != null)
            menuItemAllergens.Add(
                new MenuItemAllergen
                {
                    MenuItemId = grilledChickenWrap.Id,
                    AllergenId = soyAllergen.Id,
                }
            );

        // Veggie Salad Bowl: Sesame
        if (sesameAllergen != null)
            menuItemAllergens.Add(
                new MenuItemAllergen
                {
                    MenuItemId = veggieSaladBowl.Id,
                    AllergenId = sesameAllergen.Id,
                }
            );

        // Spicy Ramen: Wheat/Gluten, Eggs, Soy
        if (wheatAllergen != null)
            menuItemAllergens.Add(
                new MenuItemAllergen { MenuItemId = spicyRamen.Id, AllergenId = wheatAllergen.Id }
            );
        if (eggsAllergen != null)
            menuItemAllergens.Add(
                new MenuItemAllergen { MenuItemId = spicyRamen.Id, AllergenId = eggsAllergen.Id }
            );
        if (soyAllergen != null)
            menuItemAllergens.Add(
                new MenuItemAllergen { MenuItemId = spicyRamen.Id, AllergenId = soyAllergen.Id }
            );

        // Chocolate Brownie: Milk/Dairy, Wheat/Gluten, Eggs
        if (milkAllergen != null)
            menuItemAllergens.Add(
                new MenuItemAllergen
                {
                    MenuItemId = chocolateBrownie.Id,
                    AllergenId = milkAllergen.Id,
                }
            );
        if (wheatAllergen != null)
            menuItemAllergens.Add(
                new MenuItemAllergen
                {
                    MenuItemId = chocolateBrownie.Id,
                    AllergenId = wheatAllergen.Id,
                }
            );
        if (eggsAllergen != null)
            menuItemAllergens.Add(
                new MenuItemAllergen
                {
                    MenuItemId = chocolateBrownie.Id,
                    AllergenId = eggsAllergen.Id,
                }
            );

        context.MenuItemAllergens.AddRange(menuItemAllergens);

        // Add dietary restriction associations
        var menuItemDietaryRestrictions = new List<MenuItemDietaryRestriction>();

        // Classic Cheeseburger: Carnivore
        if (carnivore != null)
            menuItemDietaryRestrictions.Add(
                new MenuItemDietaryRestriction
                {
                    MenuItemId = classicCheeseburger.Id,
                    DietaryRestrictionId = carnivore.Id,
                }
            );

        // Grilled Chicken Wrap: Carnivore
        if (carnivore != null)
            menuItemDietaryRestrictions.Add(
                new MenuItemDietaryRestriction
                {
                    MenuItemId = grilledChickenWrap.Id,
                    DietaryRestrictionId = carnivore.Id,
                }
            );

        // Veggie Salad Bowl: Vegetarian, Vegan, Gluten-Free, Lactose-Free
        if (vegetarian != null)
            menuItemDietaryRestrictions.Add(
                new MenuItemDietaryRestriction
                {
                    MenuItemId = veggieSaladBowl.Id,
                    DietaryRestrictionId = vegetarian.Id,
                }
            );
        if (vegan != null)
            menuItemDietaryRestrictions.Add(
                new MenuItemDietaryRestriction
                {
                    MenuItemId = veggieSaladBowl.Id,
                    DietaryRestrictionId = vegan.Id,
                }
            );
        if (glutenFree != null)
            menuItemDietaryRestrictions.Add(
                new MenuItemDietaryRestriction
                {
                    MenuItemId = veggieSaladBowl.Id,
                    DietaryRestrictionId = glutenFree.Id,
                }
            );
        if (lactoseFree != null)
            menuItemDietaryRestrictions.Add(
                new MenuItemDietaryRestriction
                {
                    MenuItemId = veggieSaladBowl.Id,
                    DietaryRestrictionId = lactoseFree.Id,
                }
            );

        // Spicy Ramen: Vegetarian (if no meat in broth - adjust based on recipe)
        if (vegetarian != null)
            menuItemDietaryRestrictions.Add(
                new MenuItemDietaryRestriction
                {
                    MenuItemId = spicyRamen.Id,
                    DietaryRestrictionId = vegetarian.Id,
                }
            );

        // Chocolate Brownie: Vegetarian
        if (vegetarian != null)
            menuItemDietaryRestrictions.Add(
                new MenuItemDietaryRestriction
                {
                    MenuItemId = chocolateBrownie.Id,
                    DietaryRestrictionId = vegetarian.Id,
                }
            );

        context.MenuItemDietaryRestrictions.AddRange(menuItemDietaryRestrictions);
        await context.SaveChangesAsync();
    }

    private static async Task AddAssociationsToExistingItems(
        CampusDbContext context,
        List<MenuItem> existingItems
    )
    {
        // Load allergens
        var allergens = await context.Allergens.ToListAsync();
        var milkAllergen = allergens.FirstOrDefault(a => a.Name == "Milk/Dairy");
        var wheatAllergen = allergens.FirstOrDefault(a => a.Name == "Wheat/Gluten");
        var eggsAllergen = allergens.FirstOrDefault(a => a.Name == "Eggs");
        var soyAllergen = allergens.FirstOrDefault(a => a.Name == "Soy");
        var sesameAllergen = allergens.FirstOrDefault(a => a.Name == "Sesame");

        // Load dietary restrictions
        var dietaryRestrictions = await context.DietaryRestrictions.ToListAsync();
        var vegetarian = dietaryRestrictions.FirstOrDefault(d => d.Name == "Vegetarian");
        var vegan = dietaryRestrictions.FirstOrDefault(d => d.Name == "Vegan");
        var carnivore = dietaryRestrictions.FirstOrDefault(d => d.Name == "Carnivore");
        var glutenFree = dietaryRestrictions.FirstOrDefault(d => d.Name == "Gluten-Free");
        var lactoseFree = dietaryRestrictions.FirstOrDefault(d => d.Name == "Lactose-Free");

        var menuItemAllergens = new List<MenuItemAllergen>();
        var menuItemDietaryRestrictions = new List<MenuItemDietaryRestriction>();

        foreach (var item in existingItems)
        {
            // Associate allergens and dietary restrictions based on item name/type
            if (
                item.Name.Contains("Burger", StringComparison.OrdinalIgnoreCase)
                || item.Name.Contains("Cheeseburger", StringComparison.OrdinalIgnoreCase)
            )
            {
                // Burgers: Milk/Dairy, Wheat/Gluten, Eggs, Soy, Carnivore
                if (milkAllergen != null)
                    menuItemAllergens.Add(
                        new MenuItemAllergen { MenuItemId = item.Id, AllergenId = milkAllergen.Id }
                    );
                if (wheatAllergen != null)
                    menuItemAllergens.Add(
                        new MenuItemAllergen { MenuItemId = item.Id, AllergenId = wheatAllergen.Id }
                    );
                if (eggsAllergen != null)
                    menuItemAllergens.Add(
                        new MenuItemAllergen { MenuItemId = item.Id, AllergenId = eggsAllergen.Id }
                    );
                if (soyAllergen != null)
                    menuItemAllergens.Add(
                        new MenuItemAllergen { MenuItemId = item.Id, AllergenId = soyAllergen.Id }
                    );
                if (carnivore != null)
                    menuItemDietaryRestrictions.Add(
                        new MenuItemDietaryRestriction
                        {
                            MenuItemId = item.Id,
                            DietaryRestrictionId = carnivore.Id,
                        }
                    );
            }
            else if (
                item.Name.Contains("Chicken", StringComparison.OrdinalIgnoreCase)
                || item.Name.Contains("Wrap", StringComparison.OrdinalIgnoreCase)
            )
            {
                // Chicken/Wraps: Wheat/Gluten, Soy, Carnivore
                if (wheatAllergen != null)
                    menuItemAllergens.Add(
                        new MenuItemAllergen { MenuItemId = item.Id, AllergenId = wheatAllergen.Id }
                    );
                if (soyAllergen != null)
                    menuItemAllergens.Add(
                        new MenuItemAllergen { MenuItemId = item.Id, AllergenId = soyAllergen.Id }
                    );
                if (carnivore != null)
                    menuItemDietaryRestrictions.Add(
                        new MenuItemDietaryRestriction
                        {
                            MenuItemId = item.Id,
                            DietaryRestrictionId = carnivore.Id,
                        }
                    );
            }
            else if (
                item.Name.Contains("Veggie", StringComparison.OrdinalIgnoreCase)
                || item.Name.Contains("Salad", StringComparison.OrdinalIgnoreCase)
            )
            {
                // Veggie/Salads: Sesame, Vegetarian, Vegan, Gluten-Free, Lactose-Free
                if (sesameAllergen != null)
                    menuItemAllergens.Add(
                        new MenuItemAllergen
                        {
                            MenuItemId = item.Id,
                            AllergenId = sesameAllergen.Id,
                        }
                    );
                if (vegetarian != null)
                    menuItemDietaryRestrictions.Add(
                        new MenuItemDietaryRestriction
                        {
                            MenuItemId = item.Id,
                            DietaryRestrictionId = vegetarian.Id,
                        }
                    );
                if (vegan != null)
                    menuItemDietaryRestrictions.Add(
                        new MenuItemDietaryRestriction
                        {
                            MenuItemId = item.Id,
                            DietaryRestrictionId = vegan.Id,
                        }
                    );
                if (glutenFree != null)
                    menuItemDietaryRestrictions.Add(
                        new MenuItemDietaryRestriction
                        {
                            MenuItemId = item.Id,
                            DietaryRestrictionId = glutenFree.Id,
                        }
                    );
                if (lactoseFree != null)
                    menuItemDietaryRestrictions.Add(
                        new MenuItemDietaryRestriction
                        {
                            MenuItemId = item.Id,
                            DietaryRestrictionId = lactoseFree.Id,
                        }
                    );
            }
            else if (
                item.Name.Contains("Ramen", StringComparison.OrdinalIgnoreCase)
                || item.Name.Contains("Noodles", StringComparison.OrdinalIgnoreCase)
            )
            {
                // Ramen/Noodles: Wheat/Gluten, Eggs, Soy, Vegetarian
                if (wheatAllergen != null)
                    menuItemAllergens.Add(
                        new MenuItemAllergen { MenuItemId = item.Id, AllergenId = wheatAllergen.Id }
                    );
                if (eggsAllergen != null)
                    menuItemAllergens.Add(
                        new MenuItemAllergen { MenuItemId = item.Id, AllergenId = eggsAllergen.Id }
                    );
                if (soyAllergen != null)
                    menuItemAllergens.Add(
                        new MenuItemAllergen { MenuItemId = item.Id, AllergenId = soyAllergen.Id }
                    );
                if (vegetarian != null)
                    menuItemDietaryRestrictions.Add(
                        new MenuItemDietaryRestriction
                        {
                            MenuItemId = item.Id,
                            DietaryRestrictionId = vegetarian.Id,
                        }
                    );
            }
            else if (
                item.Name.Contains("Brownie", StringComparison.OrdinalIgnoreCase)
                || item.Name.Contains("Dessert", StringComparison.OrdinalIgnoreCase)
                || item.Name.Contains("Chocolate", StringComparison.OrdinalIgnoreCase)
            )
            {
                // Desserts: Milk/Dairy, Wheat/Gluten, Eggs, Vegetarian
                if (milkAllergen != null)
                    menuItemAllergens.Add(
                        new MenuItemAllergen { MenuItemId = item.Id, AllergenId = milkAllergen.Id }
                    );
                if (wheatAllergen != null)
                    menuItemAllergens.Add(
                        new MenuItemAllergen { MenuItemId = item.Id, AllergenId = wheatAllergen.Id }
                    );
                if (eggsAllergen != null)
                    menuItemAllergens.Add(
                        new MenuItemAllergen { MenuItemId = item.Id, AllergenId = eggsAllergen.Id }
                    );
                if (vegetarian != null)
                    menuItemDietaryRestrictions.Add(
                        new MenuItemDietaryRestriction
                        {
                            MenuItemId = item.Id,
                            DietaryRestrictionId = vegetarian.Id,
                        }
                    );
            }
        }

        if (menuItemAllergens.Any())
            context.MenuItemAllergens.AddRange(menuItemAllergens);
        if (menuItemDietaryRestrictions.Any())
            context.MenuItemDietaryRestrictions.AddRange(menuItemDietaryRestrictions);

        await context.SaveChangesAsync();
    }
}
