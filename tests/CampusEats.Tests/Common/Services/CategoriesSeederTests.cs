using CampusEats.Api.Common.Services;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Tests.Common.Services;

public class CategoriesSeederTests
{
    private readonly DbContextOptions<CampusDbContext> _options;

    public CategoriesSeederTests()
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
        await CategoriesSeeder.SeedCategories(context);

        // Assert
        var categories = await context.Categories.ToListAsync();
        categories.Should().NotBeEmpty();
        categories.Should().Contain(c => c.Name == "Burgers");
        categories.Should().Contain(c => c.Name == "Wraps");
        categories.Should().Contain(c => c.Name == "Salads");
        categories.Should().Contain(c => c.Name == "Noodles");
        categories.Should().Contain(c => c.Name == "Desserts");
    }

    [Fact]
    public async Task GivenExistingData_WhenSeeding_ThenDoesNotDuplicate()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);

        // Seed first time
        await CategoriesSeeder.SeedCategories(context);
        var firstCount = await context.Categories.CountAsync();

        // Act - Seed second time
        await CategoriesSeeder.SeedCategories(context);

        // Assert
        var secondCount = await context.Categories.CountAsync();
        secondCount.Should().Be(firstCount);
    }

    [Fact]
    public async Task GivenEmptyDatabase_WhenSeeding_ThenAllCategoriesAreActive()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);

        // Act
        await CategoriesSeeder.SeedCategories(context);

        // Assert
        var categories = await context.Categories.ToListAsync();
        categories.Should().OnlyContain(c => c.IsActive);
    }

    [Fact]
    public async Task GivenEmptyDatabase_WhenSeeding_ThenCategoriesHaveUniqueIds()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);

        // Act
        await CategoriesSeeder.SeedCategories(context);

        // Assert
        var categories = await context.Categories.ToListAsync();
        var distinctIds = categories.Select(c => c.Id).Distinct().ToList();
        distinctIds.Should().HaveCount(categories.Count);
    }
}
