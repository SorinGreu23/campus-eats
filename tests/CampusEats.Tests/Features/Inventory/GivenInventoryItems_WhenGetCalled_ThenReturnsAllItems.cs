using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Inventory;
using CampusEats.Api.Features.Inventory.Get;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace CampusEats.Tests.Features.Inventory;

public class GivenInventoryItems_WhenGetCalled_ThenReturnsAllItems
{
    private CampusDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new CampusDbContext(options);
    }

    [Fact]
    public async Task GivenMultipleInventoryItems_WhenHandleIsCalled_ThenReturnsAllItems()
    {
        // Arrange
        await using var context = CreateContext();
        var items = new List<InventoryItem>
        {
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Flour",
                Unit = "kg",
                CurrentQuantity = 50m,
                MinimumQuantity = 10m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Eggs",
                Unit = "dozen",
                CurrentQuantity = 20m,
                MinimumQuantity = 5m,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        };
        await context.InventoryItems.AddRangeAsync(items);
        await context.SaveChangesAsync();

        var handler = new GetInventoryItemsHandler(context);
        var request = new GetInventoryItemsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        var okResult = result as Ok<List<InventoryItemDto>>;
        okResult.ShouldNotBeNull();
        var resultItems = okResult.Value;
        resultItems.ShouldNotBeNull();
        resultItems.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GivenNoInventoryItems_WhenHandleIsCalled_ThenReturnsEmptyList()
    {
        // Arrange
        await using var context = CreateContext();
        var handler = new GetInventoryItemsHandler(context);
        var request = new GetInventoryItemsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        var okResult = result as Ok<List<InventoryItemDto>>;
        okResult.ShouldNotBeNull();
        var resultItems = okResult.Value;
        resultItems.ShouldNotBeNull();
        resultItems.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GivenItemsBelowMinimum_WhenHandleIsCalled_ThenMarksAsLowStock()
    {
        // Arrange
        await using var context = CreateContext();
        var item = new InventoryItem
        {
            Id = Guid.NewGuid(),
            Name = "Milk",
            Unit = "L",
            CurrentQuantity = 3m,
            MinimumQuantity = 10m,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        await context.InventoryItems.AddAsync(item);
        await context.SaveChangesAsync();

        var handler = new GetInventoryItemsHandler(context);
        var request = new GetInventoryItemsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        var okResult = result as Ok<List<InventoryItemDto>>;
        okResult.ShouldNotBeNull();
        var resultItems = okResult.Value;
        resultItems.ShouldNotBeNull();
        resultItems.Count.ShouldBe(1);
        
        var firstItem = resultItems.First();
        firstItem.IsLowStock.ShouldBeTrue();
    }

    [Fact]
    public async Task GivenItemsOrderedByName_WhenHandleIsCalled_ThenReturnsItemsInAlphabeticalOrder()
    {
        // Arrange
        await using var context = CreateContext();
        var items = new List<InventoryItem>
        {
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Zucchini",
                Unit = "kg",
                CurrentQuantity = 10m,
                MinimumQuantity = 5m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Apples",
                Unit = "kg",
                CurrentQuantity = 20m,
                MinimumQuantity = 5m,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = "Milk",
                Unit = "L",
                CurrentQuantity = 15m,
                MinimumQuantity = 5m,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        };
        await context.InventoryItems.AddRangeAsync(items);
        await context.SaveChangesAsync();

        var handler = new GetInventoryItemsHandler(context);
        var request = new GetInventoryItemsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        var okResult = result as Ok<List<InventoryItemDto>>;
        okResult.ShouldNotBeNull();
        var resultItems = okResult.Value;
        resultItems.ShouldNotBeNull();
        resultItems.Count.ShouldBe(3);
        
        resultItems[0].Name.ShouldBe("Apples");
        resultItems[1].Name.ShouldBe("Milk");
        resultItems[2].Name.ShouldBe("Zucchini");
    }
}
