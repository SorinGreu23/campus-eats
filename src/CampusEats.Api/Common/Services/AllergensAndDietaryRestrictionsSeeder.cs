using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Common.Services;

/// <summary>
/// Seeds the database with common allergens and dietary restrictions
/// </summary>
public static class AllergensAndDietaryRestrictionsSeeder
{
    public static async Task SeedAllergensAndDietaryRestrictions(CampusDbContext context)
    {
        await SeedAllergens(context);
        await SeedDietaryRestrictions(context);
    }

    private static async Task SeedAllergens(CampusDbContext context)
    {
        if (await context.Allergens.AnyAsync())
        {
            return; // Already seeded
        }

        var allergens = new List<Allergen>
        {
            new Allergen
            {
                Id = Guid.NewGuid(),
                Name = "Peanuts",
                Description = "Contains or may contain peanuts",
                Icon = "🥜",
            },
            new Allergen
            {
                Id = Guid.NewGuid(),
                Name = "Tree Nuts",
                Description = "Contains or may contain tree nuts (almonds, walnuts, cashews, etc.)",
                Icon = "🌰",
            },
            new Allergen
            {
                Id = Guid.NewGuid(),
                Name = "Milk/Dairy",
                Description = "Contains milk or dairy products",
                Icon = "🥛",
            },
            new Allergen
            {
                Id = Guid.NewGuid(),
                Name = "Eggs",
                Description = "Contains eggs or egg products",
                Icon = "🥚",
            },
            new Allergen
            {
                Id = Guid.NewGuid(),
                Name = "Soy",
                Description = "Contains soy or soy products",
                Icon = "🫘",
            },
            new Allergen
            {
                Id = Guid.NewGuid(),
                Name = "Wheat/Gluten",
                Description = "Contains wheat, gluten, or gluten-containing grains",
                Icon = "🌾",
            },
            new Allergen
            {
                Id = Guid.NewGuid(),
                Name = "Fish",
                Description = "Contains fish or fish products",
                Icon = "🐟",
            },
            new Allergen
            {
                Id = Guid.NewGuid(),
                Name = "Shellfish",
                Description = "Contains shellfish (shrimp, crab, lobster, etc.)",
                Icon = "🦐",
            },
            new Allergen
            {
                Id = Guid.NewGuid(),
                Name = "Sesame",
                Description = "Contains sesame seeds or sesame oil",
                Icon = "🫑",
            },
        };

        context.Allergens.AddRange(allergens);
        await context.SaveChangesAsync();
    }

    private static async Task SeedDietaryRestrictions(CampusDbContext context)
    {
        if (await context.DietaryRestrictions.AnyAsync())
        {
            return; // Already seeded
        }

        var dietaryRestrictions = new List<DietaryRestriction>
        {
            new DietaryRestriction
            {
                Id = Guid.NewGuid(),
                Name = "Vegetarian",
                Description = "Does not contain meat or fish",
                Icon = "🥗",
            },
            new DietaryRestriction
            {
                Id = Guid.NewGuid(),
                Name = "Vegan",
                Description = "Does not contain any animal products (meat, dairy, eggs, honey)",
                Icon = "🌱",
            },
            new DietaryRestriction
            {
                Id = Guid.NewGuid(),
                Name = "Carnivore",
                Description = "Contains meat or animal products",
                Icon = "🥩",
            },
            new DietaryRestriction
            {
                Id = Guid.NewGuid(),
                Name = "Pescatarian",
                Description = "Contains fish but no other meat",
                Icon = "🐟",
            },
            new DietaryRestriction
            {
                Id = Guid.NewGuid(),
                Name = "Keto",
                Description = "Low-carb, high-fat diet friendly",
                Icon = "🥑",
            },
            new DietaryRestriction
            {
                Id = Guid.NewGuid(),
                Name = "Paleo",
                Description = "Based on foods presumed to be available to paleolithic humans",
                Icon = "🍖",
            },
            new DietaryRestriction
            {
                Id = Guid.NewGuid(),
                Name = "Gluten-Free",
                Description = "Does not contain gluten",
                Icon = "🌾",
            },
            new DietaryRestriction
            {
                Id = Guid.NewGuid(),
                Name = "Halal",
                Description = "Prepared according to Islamic law",
                Icon = "☪️",
            },
            new DietaryRestriction
            {
                Id = Guid.NewGuid(),
                Name = "Kosher",
                Description = "Prepared according to Jewish dietary law",
                Icon = "✡️",
            },
            new DietaryRestriction
            {
                Id = Guid.NewGuid(),
                Name = "Lactose-Free",
                Description = "Does not contain lactose or dairy products",
                Icon = "🚫🥛",
            },
        };

        context.DietaryRestrictions.AddRange(dietaryRestrictions);
        await context.SaveChangesAsync();
    }
}
