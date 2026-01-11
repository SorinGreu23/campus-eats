using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Common.Services;

public static class MenuItemsSeeder
{
  private const string Vegetarian = "Vegetarian";
  private const string Vegan = "Vegan";
  private const string Carnivore = "Carnivore";
  private const string Sesame = "Sesame";
  private const string Wheat = "Wheat";

  public static async Task SeedMenuItems(CampusDbContext context)
    {
        await EnsureDependenciesExist(context);

        if (await HasExistingAssociations(context))
        {
            return;
        }

        var existingItems = await context.MenuItems.ToListAsync();
        if (existingItems.Count != 0)
        {
            await AddAssociationsToExistingItems(context, existingItems);
            return;
        }

        await CreateNewMenuItemsWithAssociations(context);
    }

    private static async Task EnsureDependenciesExist(CampusDbContext context)
    {
        if (!await context.Categories.AnyAsync())
        {
            await CategoriesSeeder.SeedCategories(context);
        }

        if (!await context.Allergens.AnyAsync() || !await context.DietaryRestrictions.AnyAsync())
        {
            await AllergensAndDietaryRestrictionsSeeder.SeedAllergensAndDietaryRestrictions(
                context
            );
        }
    }

    private static async Task<bool> HasExistingAssociations(CampusDbContext context)
    {
        return await context
            .MenuItems.Include(m => m.MenuItemAllergens)
            .Include(m => m.MenuItemDietaryRestrictions)
            .AnyAsync(m => m.MenuItemAllergens.Any() || m.MenuItemDietaryRestrictions.Any());
    }

    private static async Task CreateNewMenuItemsWithAssociations(CampusDbContext context)
    {
        var categories = await context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
        var allergens = await context.Allergens.ToListAsync();
        var dietaryRestrictions = await context.DietaryRestrictions.ToListAsync();

        var menuItems = CreateMenuItems(categories);
        context.MenuItems.AddRange(menuItems);
        await context.SaveChangesAsync();

        var allergenAssociations = CreateAllergenAssociations(menuItems, allergens);
        var dietaryAssociations = CreateDietaryAssociations(menuItems, dietaryRestrictions);

        context.MenuItemAllergens.AddRange(allergenAssociations);
        context.MenuItemDietaryRestrictions.AddRange(dietaryAssociations);
        await context.SaveChangesAsync();
    }

    private static List<MenuItem> CreateMenuItems(List<Category> categories)
    {
        var now = DateTimeOffset.UtcNow;
        var burgersCat = categories.FirstOrDefault(c => c.Name == "Burgers")?.Id;
        var wrapsCat = categories.FirstOrDefault(c => c.Name == "Wraps")?.Id;
        var saladsCat = categories.FirstOrDefault(c => c.Name == "Salads")?.Id;
        var noodlesCat = categories.FirstOrDefault(c => c.Name == "Noodles")?.Id;
        var dessertsCat = categories.FirstOrDefault(c => c.Name == "Desserts")?.Id;

        return
        [
            new MenuItem
            {
                Id = Guid.NewGuid(),
                Name = "Classic Cheeseburger",
                Description = "Juicy beef patty with cheddar, lettuce, tomato, and our special sauce.",
                Price = 8.99m,
                CategoryId = burgersCat,
                ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=400&h=400&fit=crop",
                PreparationTimeMinutes = 15,
                IsAvailable = true,
                Calories = 750,
                CreatedAt = now,
                UpdatedAt = now,
            },
            new MenuItem
            {
                Id = Guid.NewGuid(),
                Name = "Grilled Chicken Wrap",
                Description = "Marinated grilled chicken with fresh greens and a light vinaigrette.",
                Price = 7.49m,
                CategoryId = wrapsCat,
                ImageUrl = "https://images.unsplash.com/photo-1626700051175-6818013e1d4f?w=400&h=400&fit=crop",
                PreparationTimeMinutes = 10,
                IsAvailable = true,
                Calories = 420,
                CreatedAt = now,
                UpdatedAt = now,
            },
            new MenuItem
            {
                Id = Guid.NewGuid(),
                Name = "Veggie Salad Bowl",
                Description = "Seasonal vegetables, quinoa, and a lemon-tahini dressing.",
                Price = 6.50m,
                CategoryId = saladsCat,
                ImageUrl = "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=400&h=400&fit=crop",
                PreparationTimeMinutes = 8,
                IsAvailable = true,
                Calories = 320,
                CreatedAt = now,
                UpdatedAt = now,
            },
            new MenuItem
            {
                Id = Guid.NewGuid(),
                Name = "Spicy Ramen",
                Description = "Hot and savory broth with noodles, egg, and spicy chili oil.",
                Price = 9.25m,
                CategoryId = noodlesCat,
                ImageUrl = "https://images.unsplash.com/photo-1569718212165-3a8278d5f624?w=400&h=400&fit=crop",
                PreparationTimeMinutes = 12,
                IsAvailable = true,
                Calories = 610,
                CreatedAt = now,
                UpdatedAt = now,
            },
            new MenuItem
            {
                Id = Guid.NewGuid(),
                Name = "Chocolate Brownie",
                Description = "Warm chocolate brownie served with vanilla ice cream.",
                Price = 3.75m,
                CategoryId = dessertsCat,
                ImageUrl = "https://images.unsplash.com/photo-1606313564200-e75d5e30476c?w=400&h=400&fit=crop",
                PreparationTimeMinutes = 5,
                IsAvailable = true,
                Calories = 450,
                CreatedAt = now,
                UpdatedAt = now,
            }
        ];
    }

    private static List<MenuItemAllergen> CreateAllergenAssociations(
        List<MenuItem> menuItems,
        List<Allergen> allergens
    )
    {
        var associations = new List<MenuItemAllergen>();
        var milkAllergen = allergens.FirstOrDefault(a => a.Name == "Milk/Dairy");
        var wheatAllergen = allergens.FirstOrDefault(a => a.Name == "Wheat/Gluten");
        var eggsAllergen = allergens.FirstOrDefault(a => a.Name == "Eggs");
        var soyAllergen = allergens.FirstOrDefault(a => a.Name == "Soy");
        var sesameAllergen = allergens.FirstOrDefault(a => a.Name == Sesame);

        var cheeseburger = menuItems.FirstOrDefault(m => m.Name == "Classic Cheeseburger");
        var chickenWrap = menuItems.FirstOrDefault(m => m.Name == "Grilled Chicken Wrap");
        var veggieSalad = menuItems.FirstOrDefault(m => m.Name == "Veggie Salad Bowl");
        var ramen = menuItems.FirstOrDefault(m => m.Name == "Spicy Ramen");
        var brownie = menuItems.FirstOrDefault(m => m.Name == "Chocolate Brownie");

        AddAllergenIfExists(associations, cheeseburger, new[] { milkAllergen, wheatAllergen, eggsAllergen, soyAllergen });
        AddAllergenIfExists(associations, chickenWrap, new[] { wheatAllergen, soyAllergen });
        AddAllergenIfExists(associations, veggieSalad, new[] { sesameAllergen });
        AddAllergenIfExists(associations, ramen, new[] { wheatAllergen, eggsAllergen, soyAllergen });
        AddAllergenIfExists(associations, brownie, new[] { milkAllergen, wheatAllergen, eggsAllergen });

        return associations;
    }

    private static void AddAllergenIfExists(
        List<MenuItemAllergen> associations,
        MenuItem? menuItem,
        Allergen?[] allergens
    )
    {
        if (menuItem == null) return;

        foreach (var allergen in allergens.Where(a => a != null))
        {
            associations.Add(new MenuItemAllergen
            {
                MenuItemId = menuItem.Id,
                AllergenId = allergen!.Id
            });
        }
    }

    private static List<MenuItemDietaryRestriction> CreateDietaryAssociations(
        List<MenuItem> menuItems,
        List<DietaryRestriction> dietaryRestrictions
    )
    {
        var associations = new List<MenuItemDietaryRestriction>();
        var vegetarian = dietaryRestrictions.FirstOrDefault(d => d.Name == Vegetarian);
        var vegan = dietaryRestrictions.FirstOrDefault(d => d.Name == Vegan);
        var carnivore = dietaryRestrictions.FirstOrDefault(d => d.Name == Carnivore);
        var glutenFree = dietaryRestrictions.FirstOrDefault(d => d.Name == "Gluten-Free");
        var lactoseFree = dietaryRestrictions.FirstOrDefault(d => d.Name == "Lactose-Free");

        var cheeseburger = menuItems.FirstOrDefault(m => m.Name == "Classic Cheeseburger");
        var chickenWrap = menuItems.FirstOrDefault(m => m.Name == "Grilled Chicken Wrap");
        var veggieSalad = menuItems.FirstOrDefault(m => m.Name == "Veggie Salad Bowl");
        var ramen = menuItems.FirstOrDefault(m => m.Name == "Spicy Ramen");
        var brownie = menuItems.FirstOrDefault(m => m.Name == "Chocolate Brownie");

        AddDietaryIfExists(associations, cheeseburger, new[] { carnivore });
        AddDietaryIfExists(associations, chickenWrap, new[] { carnivore });
        AddDietaryIfExists(associations, veggieSalad, new[] { vegetarian, vegan, glutenFree, lactoseFree });
        AddDietaryIfExists(associations, ramen, new[] { vegetarian });
        AddDietaryIfExists(associations, brownie, new[] { vegetarian });

        return associations;
    }

    private static void AddDietaryIfExists(
        List<MenuItemDietaryRestriction> associations,
        MenuItem? menuItem,
        DietaryRestriction?[] restrictions
    )
    {
        if (menuItem == null) return;

        foreach (var restriction in restrictions.Where(r => r != null))
        {
            associations.Add(new MenuItemDietaryRestriction
            {
                MenuItemId = menuItem.Id,
                DietaryRestrictionId = restriction!.Id
            });
        }
    }

    private static async Task AddAssociationsToExistingItems(
        CampusDbContext context,
        List<MenuItem> existingItems
    )
    {
        var allergens = await context.Allergens.ToListAsync();
        var dietaryRestrictions = await context.DietaryRestrictions.ToListAsync();

        var allergenLookup = CreateAllergenLookup(allergens);
        var dietaryLookup = CreateDietaryRestrictionLookup(dietaryRestrictions);

        var menuItemAllergens = new List<MenuItemAllergen>();
        var menuItemDietaryRestrictions = new List<MenuItemDietaryRestriction>();

        foreach (var item in existingItems)
        {
            AddAssociationsForMenuItem(item, allergenLookup, dietaryLookup, menuItemAllergens, menuItemDietaryRestrictions);
        }

        if (menuItemAllergens.Count != 0)
            context.MenuItemAllergens.AddRange(menuItemAllergens);
        if (menuItemDietaryRestrictions.Count != 0)
            context.MenuItemDietaryRestrictions.AddRange(menuItemDietaryRestrictions);

        await context.SaveChangesAsync();
    }

    private static Dictionary<string, Allergen?> CreateAllergenLookup(List<Allergen> allergens)
    {
        return new Dictionary<string, Allergen?>
        {
            ["Milk"] = allergens.FirstOrDefault(a => a.Name == "Milk/Dairy"),
            [Wheat] = allergens.FirstOrDefault(a => a.Name == "Wheat/Gluten"),
            ["Eggs"] = allergens.FirstOrDefault(a => a.Name == "Eggs"),
            ["Soy"] = allergens.FirstOrDefault(a => a.Name == "Soy"),
            [Sesame] = allergens.FirstOrDefault(a => a.Name == Sesame)
        };
    }

    private static Dictionary<string, DietaryRestriction?> CreateDietaryRestrictionLookup(List<DietaryRestriction> dietaryRestrictions)
    {
        return new Dictionary<string, DietaryRestriction?>
        {
            [Vegetarian] = dietaryRestrictions.FirstOrDefault(d => d.Name == Vegetarian),
            [Vegan] = dietaryRestrictions.FirstOrDefault(d => d.Name == Vegan),
            [Carnivore] = dietaryRestrictions.FirstOrDefault(d => d.Name == Carnivore),
            ["GlutenFree"] = dietaryRestrictions.FirstOrDefault(d => d.Name == "Gluten-Free"),
            ["LactoseFree"] = dietaryRestrictions.FirstOrDefault(d => d.Name == "Lactose-Free")
        };
    }

    private static void AddAssociationsForMenuItem(
        MenuItem item,
        Dictionary<string, Allergen?> allergenLookup,
        Dictionary<string, DietaryRestriction?> dietaryLookup,
        List<MenuItemAllergen> menuItemAllergens,
        List<MenuItemDietaryRestriction> menuItemDietaryRestrictions)
    {
        if (IsBurgerItem(item.Name))
        {
            AddBurgerAssociations(item, allergenLookup, dietaryLookup, menuItemAllergens, menuItemDietaryRestrictions);
        }
        else if (IsChickenOrWrapItem(item.Name))
        {
            AddChickenWrapAssociations(item, allergenLookup, dietaryLookup, menuItemAllergens, menuItemDietaryRestrictions);
        }
        else if (IsVeggieOrSaladItem(item.Name))
        {
            AddVeggieSaladAssociations(item, allergenLookup, dietaryLookup, menuItemAllergens, menuItemDietaryRestrictions);
        }
        else if (IsRamenOrNoodleItem(item.Name))
        {
            AddRamenNoodleAssociations(item, allergenLookup, dietaryLookup, menuItemAllergens, menuItemDietaryRestrictions);
        }
        else if (IsDessertItem(item.Name))
        {
            AddDessertAssociations(item, allergenLookup, dietaryLookup, menuItemAllergens, menuItemDietaryRestrictions);
        }
    }

    private static bool IsBurgerItem(string name) =>
        name.Contains("Burger", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("Cheeseburger", StringComparison.OrdinalIgnoreCase);

    private static bool IsChickenOrWrapItem(string name) =>
        name.Contains("Chicken", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("Wrap", StringComparison.OrdinalIgnoreCase);

    private static bool IsVeggieOrSaladItem(string name) =>
        name.Contains("Veggie", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("Salad", StringComparison.OrdinalIgnoreCase);

    private static bool IsRamenOrNoodleItem(string name) =>
        name.Contains("Ramen", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("Noodles", StringComparison.OrdinalIgnoreCase);

    private static bool IsDessertItem(string name) =>
        name.Contains("Brownie", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("Dessert", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("Chocolate", StringComparison.OrdinalIgnoreCase);

    private static void AddBurgerAssociations(
        MenuItem item,
        Dictionary<string, Allergen?> allergenLookup,
        Dictionary<string, DietaryRestriction?> dietaryLookup,
        List<MenuItemAllergen> menuItemAllergens,
        List<MenuItemDietaryRestriction> menuItemDietaryRestrictions)
    {
        AddAllergenToList(menuItemAllergens, item.Id, allergenLookup["Milk"]);
        AddAllergenToList(menuItemAllergens, item.Id, allergenLookup[Wheat]);
        AddAllergenToList(menuItemAllergens, item.Id, allergenLookup["Eggs"]);
        AddAllergenToList(menuItemAllergens, item.Id, allergenLookup["Soy"]);
        AddDietaryToList(menuItemDietaryRestrictions, item.Id, dietaryLookup[Carnivore]);
    }

    private static void AddChickenWrapAssociations(
        MenuItem item,
        Dictionary<string, Allergen?> allergenLookup,
        Dictionary<string, DietaryRestriction?> dietaryLookup,
        List<MenuItemAllergen> menuItemAllergens,
        List<MenuItemDietaryRestriction> menuItemDietaryRestrictions)
    {
        AddAllergenToList(menuItemAllergens, item.Id, allergenLookup[Wheat]);
        AddAllergenToList(menuItemAllergens, item.Id, allergenLookup["Soy"]);
        AddDietaryToList(menuItemDietaryRestrictions, item.Id, dietaryLookup[Carnivore]);
    }

    private static void AddVeggieSaladAssociations(
        MenuItem item,
        Dictionary<string, Allergen?> allergenLookup,
        Dictionary<string, DietaryRestriction?> dietaryLookup,
        List<MenuItemAllergen> menuItemAllergens,
        List<MenuItemDietaryRestriction> menuItemDietaryRestrictions)
    {
        AddAllergenToList(menuItemAllergens, item.Id, allergenLookup[Sesame]);
        AddDietaryToList(menuItemDietaryRestrictions, item.Id, dietaryLookup[Vegetarian]);
        AddDietaryToList(menuItemDietaryRestrictions, item.Id, dietaryLookup[Vegan]);
        AddDietaryToList(menuItemDietaryRestrictions, item.Id, dietaryLookup["GlutenFree"]);
        AddDietaryToList(menuItemDietaryRestrictions, item.Id, dietaryLookup["LactoseFree"]);
    }

    private static void AddRamenNoodleAssociations(
        MenuItem item,
        Dictionary<string, Allergen?> allergenLookup,
        Dictionary<string, DietaryRestriction?> dietaryLookup,
        List<MenuItemAllergen> menuItemAllergens,
        List<MenuItemDietaryRestriction> menuItemDietaryRestrictions)
    {
        AddAllergenToList(menuItemAllergens, item.Id, allergenLookup[Wheat]);
        AddAllergenToList(menuItemAllergens, item.Id, allergenLookup["Eggs"]);
        AddAllergenToList(menuItemAllergens, item.Id, allergenLookup["Soy"]);
        AddDietaryToList(menuItemDietaryRestrictions, item.Id, dietaryLookup[Vegetarian]);
    }

    private static void AddDessertAssociations(
        MenuItem item,
        Dictionary<string, Allergen?> allergenLookup,
        Dictionary<string, DietaryRestriction?> dietaryLookup,
        List<MenuItemAllergen> menuItemAllergens,
        List<MenuItemDietaryRestriction> menuItemDietaryRestrictions)
    {
        AddAllergenToList(menuItemAllergens, item.Id, allergenLookup["Milk"]);
        AddAllergenToList(menuItemAllergens, item.Id, allergenLookup[Wheat]);
        AddAllergenToList(menuItemAllergens, item.Id, allergenLookup["Eggs"]);
        AddDietaryToList(menuItemDietaryRestrictions, item.Id, dietaryLookup[Vegetarian]);
    }

    private static void AddAllergenToList(List<MenuItemAllergen> list, Guid menuItemId, Allergen? allergen)
    {
        if (allergen != null)
        {
            list.Add(new MenuItemAllergen { MenuItemId = menuItemId, AllergenId = allergen.Id });
        }
    }

    private static void AddDietaryToList(List<MenuItemDietaryRestriction> list, Guid menuItemId, DietaryRestriction? restriction)
    {
        if (restriction != null)
        {
            list.Add(new MenuItemDietaryRestriction { MenuItemId = menuItemId, DietaryRestrictionId = restriction.Id });
        }
    }
}
