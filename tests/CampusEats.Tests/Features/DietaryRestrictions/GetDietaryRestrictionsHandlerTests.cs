using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.DietaryRestrictions;
using CampusEats.Api.Features.Menu;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace CampusEats.Tests.Features.DietaryRestrictions;

public class GetDietaryRestrictionsHandlerTests
{
    private static CampusDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new CampusDbContext(options);
    }

    [Fact]
    public async Task GivenNoDietaryRestrictions_WhenHandleIsCalled_ThenReturnsEmptyList()
    {
        // Arrange
        await using var context = CreateContext();
        var handler = new GetDietaryRestrictionsHandler(context);
        var request = new GetDietaryRestrictionsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<List<DietaryRestrictionDto>>>();
        var okResult = (Ok<List<DietaryRestrictionDto>>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GivenMultipleDietaryRestrictions_WhenHandleIsCalled_ThenReturnsAllRestrictions()
    {
        // Arrange
        await using var context = CreateContext();

        var restriction1 = new DietaryRestriction
        {
            Id = Guid.NewGuid(),
            Name = "Vegan",
            Description = "No animal products",
            Icon = "🌱"
        };

        var restriction2 = new DietaryRestriction
        {
            Id = Guid.NewGuid(),
            Name = "Vegetarian",
            Description = "No meat or fish",
            Icon = "🥗"
        };

        var restriction3 = new DietaryRestriction
        {
            Id = Guid.NewGuid(),
            Name = "Halal",
            Description = "Prepared according to Islamic law",
            Icon = "☪️"
        };

        context.DietaryRestrictions.AddRange(restriction1, restriction2, restriction3);
        await context.SaveChangesAsync();

        var handler = new GetDietaryRestrictionsHandler(context);
        var request = new GetDietaryRestrictionsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<List<DietaryRestrictionDto>>>();
        var okResult = (Ok<List<DietaryRestrictionDto>>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Count.ShouldBe(3);
        okResult.Value.ShouldContain(r => r.Name == "Vegan" && r.Description == "No animal products");
        okResult.Value.ShouldContain(r => r.Name == "Vegetarian" && r.Description == "No meat or fish");
        okResult.Value.ShouldContain(r => r.Name == "Halal" && r.Description == "Prepared according to Islamic law");
    }

    [Fact]
    public async Task GivenRestrictionsWithNullFields_WhenHandleIsCalled_ThenReturnsRestrictionsWithNulls()
    {
        // Arrange
        await using var context = CreateContext();

        var restriction = new DietaryRestriction
        {
            Id = Guid.NewGuid(),
            Name = "Kosher",
            Description = null,
            Icon = null
        };

        context.DietaryRestrictions.Add(restriction);
        await context.SaveChangesAsync();

        var handler = new GetDietaryRestrictionsHandler(context);
        var request = new GetDietaryRestrictionsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Ok<List<DietaryRestrictionDto>>>();
        var okResult = (Ok<List<DietaryRestrictionDto>>)result;
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Count.ShouldBe(1);
        var dto = okResult.Value[0];
        dto.Name.ShouldBe("Kosher");
        dto.Description.ShouldBeNull();
        dto.Icon.ShouldBeNull();
    }
}
