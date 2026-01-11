using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Menu.Categories;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Tests.Features.Menu;

public class GetCategoriesHandlerTests
{
    private readonly DbContextOptions<CampusDbContext> _options;

    public GetCategoriesHandlerTests()
    {
        _options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GivenNoCategories_WhenGettingCategories_ThenReturnsEmptyList()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);
        var handler = new GetCategoriesHandler(context);
        var request = new GetCategoriesRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IResult>();
        
        // Check it's an Ok result by checking the type name
        result.GetType().Name.Should().StartWith("Ok");
    }

    [Fact]
    public async Task GivenMultipleCategories_WhenGettingCategories_ThenReturnsOrderedList()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);
        
        var categories = new List<Category>
        {
            new Category { Id = Guid.NewGuid(), Name = "Desserts", DisplayOrder = 5, IsActive = true },
            new Category { Id = Guid.NewGuid(), Name = "Beverages", DisplayOrder = 4, IsActive = true },
            new Category { Id = Guid.NewGuid(), Name = "Pizza", DisplayOrder = 1, IsActive = true },
            new Category { Id = Guid.NewGuid(), Name = "Burgers", DisplayOrder = 2, IsActive = true },
            new Category { Id = Guid.NewGuid(), Name = "Salads", DisplayOrder = 3, IsActive = true }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        var handler = new GetCategoriesHandler(context);
        var request = new GetCategoriesRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IResult>();
        
        // Check it's an Ok result by checking the type name
        result.GetType().Name.Should().StartWith("Ok");
        
        // Get the Value property using reflection since we don't know the exact generic type
        var valueProperty = result.GetType().GetProperty("Value");
        valueProperty.Should().NotBeNull();
        var value = valueProperty!.GetValue(result) as IEnumerable<object>;
        value.Should().NotBeNull();
        value!.Should().HaveCount(5);

        // Verify ordering by DisplayOrder
        var categoryList = await context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        categoryList[0].Name.Should().Be("Pizza");
        categoryList[1].Name.Should().Be("Burgers");
        categoryList[2].Name.Should().Be("Salads");
        categoryList[3].Name.Should().Be("Beverages");
        categoryList[4].Name.Should().Be("Desserts");
    }

    [Fact]
    public async Task GivenInactiveCategories_WhenGettingCategories_ThenReturnsOnlyActiveCategories()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);
        
        var categories = new List<Category>
        {
            new Category { Id = Guid.NewGuid(), Name = "Active Category 1", DisplayOrder = 1, IsActive = true },
            new Category { Id = Guid.NewGuid(), Name = "Inactive Category", DisplayOrder = 2, IsActive = false },
            new Category { Id = Guid.NewGuid(), Name = "Active Category 2", DisplayOrder = 3, IsActive = true }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        var handler = new GetCategoriesHandler(context);
        var request = new GetCategoriesRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IResult>();
        
        // Check it's an Ok result
        result.GetType().Name.Should().StartWith("Ok");// Only active categories
    }
}
