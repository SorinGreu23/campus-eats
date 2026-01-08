using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Allergens;
using CampusEats.Api.Features.Menu;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace CampusEats.Tests.Features.Allergens;

public class GetAllergensHandlerTests
{
    private CampusDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new CampusDbContext(options);
    }

    [Fact]
    public async Task GivenNoAllergens_WhenHandleIsCalled_ThenReturnsEmptyList()
    {
        // Arrange
        await using var context = CreateContext();
        var handler = new GetAllergensHandler(context);
        var request = new GetAllergensRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<List<AllergenDto>>>();
        var okResult = (Ok<List<AllergenDto>>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GivenMultipleAllergens_WhenHandleIsCalled_ThenReturnsAllAllergens()
    {
        // Arrange
        await using var context = CreateContext();

        var allergen1 = new Allergen
        {
            Id = Guid.NewGuid(),
            Name = "Nuts",
            Description = "Tree nuts and peanuts",
            Icon = "🥜"
        };

        var allergen2 = new Allergen
        {
            Id = Guid.NewGuid(),
            Name = "Dairy",
            Description = "Milk and dairy products",
            Icon = "🥛"
        };

        var allergen3 = new Allergen
        {
            Id = Guid.NewGuid(),
            Name = "Gluten",
            Description = "Wheat and gluten-containing grains",
            Icon = "🌾"
        };

        context.Allergens.AddRange(allergen1, allergen2, allergen3);
        await context.SaveChangesAsync();

        var handler = new GetAllergensHandler(context);
        var request = new GetAllergensRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<List<AllergenDto>>>();
        var okResult = (Ok<List<AllergenDto>>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Count.ShouldBe(3);
        okResult.Value.ShouldContain(a => a.Name == "Nuts" && a.Description == "Tree nuts and peanuts");
        okResult.Value.ShouldContain(a => a.Name == "Dairy" && a.Description == "Milk and dairy products");
        okResult.Value.ShouldContain(a => a.Name == "Gluten" && a.Description == "Wheat and gluten-containing grains");
    }

    [Fact]
    public async Task GivenAllergensWithNullFields_WhenHandleIsCalled_ThenReturnsAllergensWithNulls()
    {
        // Arrange
        await using var context = CreateContext();

        var allergen = new Allergen
        {
            Id = Guid.NewGuid(),
            Name = "Shellfish",
            Description = null,
            Icon = null
        };

        context.Allergens.Add(allergen);
        await context.SaveChangesAsync();

        var handler = new GetAllergensHandler(context);
        var request = new GetAllergensRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<List<AllergenDto>>>();
        var okResult = (Ok<List<AllergenDto>>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Count.ShouldBe(1);
        var dto = okResult.Value.First();
        dto.Name.ShouldBe("Shellfish");
        dto.Description.ShouldBeNull();
        dto.Icon.ShouldBeNull();
    }
}
