using CampusEats.Api.Common.Services;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Tests.Common.Services;

public class AllergensAndDietaryRestrictionsSeederTests
{
    private readonly DbContextOptions<CampusDbContext> _options;

    public AllergensAndDietaryRestrictionsSeederTests()
    {
        _options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GivenEmptyDatabase_WhenSeeding_ThenCreatesExpectedRecords()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);

        // Act
        await AllergensAndDietaryRestrictionsSeeder.SeedAllergensAndDietaryRestrictions(context);

        // Assert
        var allergens = await context.Allergens.ToListAsync();
        var dietaryRestrictions = await context.DietaryRestrictions.ToListAsync();

        allergens.Should().NotBeEmpty();
        dietaryRestrictions.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GivenExistingData_WhenSeeding_ThenDoesNotDuplicate()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);

        // Seed first time
        await AllergensAndDietaryRestrictionsSeeder.SeedAllergensAndDietaryRestrictions(context);
        var firstAllergenCount = await context.Allergens.CountAsync();
        var firstDietaryCount = await context.DietaryRestrictions.CountAsync();

        // Act - Seed second time
        await AllergensAndDietaryRestrictionsSeeder.SeedAllergensAndDietaryRestrictions(context);

        // Assert
        var secondAllergenCount = await context.Allergens.CountAsync();
        var secondDietaryCount = await context.DietaryRestrictions.CountAsync();
        
        secondAllergenCount.Should().Be(firstAllergenCount);
        secondDietaryCount.Should().Be(firstDietaryCount);
    }

    [Fact]
    public async Task GivenEmptyDatabase_WhenSeeding_ThenAllergensContainCommonItems()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);

        // Act
        await AllergensAndDietaryRestrictionsSeeder.SeedAllergensAndDietaryRestrictions(context);

        // Assert
        var allergens = await context.Allergens.ToListAsync();
        
        allergens.Should().Contain(a => a.Name == "Peanuts");
        allergens.Should().Contain(a => a.Name == "Tree Nuts");
        allergens.Should().Contain(a => a.Name.Contains("Milk") || a.Name.Contains("Dairy"));
        allergens.Should().Contain(a => a.Name == "Soy");
        allergens.Should().Contain(a => a.Name == "Eggs");
        allergens.Should().Contain(a => a.Name == "Fish");
        allergens.Should().Contain(a => a.Name == "Shellfish");
    }

    [Fact]
    public async Task GivenEmptyDatabase_WhenSeeding_ThenDietaryRestrictionsContainCommonItems()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);

        // Act
        await AllergensAndDietaryRestrictionsSeeder.SeedAllergensAndDietaryRestrictions(context);

        // Assert
        var dietaryRestrictions = await context.DietaryRestrictions.ToListAsync();
        
        dietaryRestrictions.Should().Contain(d => d.Name == "Vegetarian");
        dietaryRestrictions.Should().Contain(d => d.Name == "Vegan");
        dietaryRestrictions.Should().Contain(d => d.Name.Contains("Gluten") && d.Name.Contains("Free"));
        dietaryRestrictions.Should().Contain(d => d.Name.Contains("Halal") || d.Name.Contains("halal"));
        dietaryRestrictions.Should().Contain(d => d.Name.Contains("Kosher") || d.Name.Contains("kosher"));
    }

    [Fact]
    public async Task GivenEmptyDatabase_WhenSeeding_ThenAllergensHaveDescriptions()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);

        // Act
        await AllergensAndDietaryRestrictionsSeeder.SeedAllergensAndDietaryRestrictions(context);

        // Assert
        var allergens = await context.Allergens.ToListAsync();
        allergens.Should().OnlyContain(a => !string.IsNullOrWhiteSpace(a.Description));
    }

    [Fact]
    public async Task GivenEmptyDatabase_WhenSeeding_ThenDietaryRestrictionsHaveDescriptions()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);

        // Act
        await AllergensAndDietaryRestrictionsSeeder.SeedAllergensAndDietaryRestrictions(context);

        // Assert
        var dietaryRestrictions = await context.DietaryRestrictions.ToListAsync();
        dietaryRestrictions.Should().OnlyContain(d => !string.IsNullOrWhiteSpace(d.Description));
    }

    [Fact]
    public async Task GivenEmptyDatabase_WhenSeeding_ThenRecordsHaveUniqueIds()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);

        // Act
        await AllergensAndDietaryRestrictionsSeeder.SeedAllergensAndDietaryRestrictions(context);

        // Assert
        var allergens = await context.Allergens.ToListAsync();
        var allergenIds = allergens.Select(a => a.Id).Distinct().ToList();
        allergenIds.Should().HaveCount(allergens.Count);

        var dietaryRestrictions = await context.DietaryRestrictions.ToListAsync();
        var dietaryIds = dietaryRestrictions.Select(d => d.Id).Distinct().ToList();
        dietaryIds.Should().HaveCount(dietaryRestrictions.Count);
    }
}
