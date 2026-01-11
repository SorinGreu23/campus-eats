using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Inventory.Use;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Tests.Features.Inventory.Use;

public class UseInventoryHandlerTests
{
    private static CampusDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CampusDbContext(options);
    }

    private static UseInventoryHandler CreateHandler(CampusDbContext db)
    {
        var validator = new UseInventoryValidator();
        return new UseInventoryHandler(db, validator);
    }

    [Fact]
    public async Task GivenValidRequest_WhenUsingInventory_ThenCreatesTransactionAndUpdatesQuantities()
    {
        await using var db = CreateDbContext();
        var handler = CreateHandler(db);

        var item = new InventoryItem
        {
            Id = Guid.NewGuid(),
            Name = "Tomatoes",
            Unit = "kg",
            CurrentQuantity = 5m,
            MinimumQuantity = 3m,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.InventoryItems.Add(item);
        await db.SaveChangesAsync();

        var request = new UseInventoryRequest(item.Id, 2m, "Prep order #123");
        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().BeAssignableTo<IResult>();
        var locationProp = result.GetType().GetProperty("Location");
        locationProp.Should().NotBeNull();
        var location = locationProp!.GetValue(result) as string;
        location.Should().NotBeNull();

        var valueProp = result.GetType().GetProperty("Value");
        valueProp.Should().NotBeNull();
        var value = valueProp!.GetValue(result)!;

        var usedProp = value.GetType().GetProperty("UsedQuantity")!;
        var remainingProp = value.GetType().GetProperty("RemainingQuantity")!;
        var isLowStockProp = value.GetType().GetProperty("IsLowStock")!;
        var isOutProp = value.GetType().GetProperty("IsOutOfStock")!;
        var performedProp = value.GetType().GetProperty("PerformedBy")!;

        usedProp.GetValue(value).Should().Be(2m);
        remainingProp.GetValue(value).Should().Be(3m);
        isLowStockProp.GetValue(value).Should().Be(true);
        isOutProp.GetValue(value).Should().Be(false);
        performedProp.GetValue(value).Should().Be("chef");

        var tx = await db.InventoryTransactions.FirstOrDefaultAsync(t => t.InventoryItemId == item.Id);
        tx.Should().NotBeNull();
        tx!.TransactionType.Should().Be("Use");
        tx.Quantity.Should().Be(-2m);
        tx.Reason.Should().Be("Prep order #123");

        var updated = await db.InventoryItems.FirstAsync(i => i.Id == item.Id);
        updated.CurrentQuantity.Should().Be(3m);
        updated.IsOutOfStock.Should().BeFalse();
    }

    [Fact]
    public async Task GivenMissingItem_WhenUsingInventory_ThenReturnsNotFound()
    {
        await using var db = CreateDbContext();
        var handler = CreateHandler(db);

        var id = Guid.NewGuid();
        var result = await handler.Handle(new UseInventoryRequest(id, 1m, null), CancellationToken.None);

        result.GetType().Name.Should().StartWith("NotFound");
        var valueProp = result.GetType().GetProperty("Value")!;
        var message = valueProp.GetValue(result) as string;
        message.Should().Be($"Inventory item with ID '{id}' was not found.");
    }

    [Fact]
    public async Task GivenOutOfStockItem_WhenUsingInventory_ThenReturnsBadRequest()
    {
        await using var db = CreateDbContext();
        var handler = CreateHandler(db);

        var item = new InventoryItem
        {
            Id = Guid.NewGuid(),
            Name = "Lettuce",
            Unit = "pcs",
            CurrentQuantity = 0m,
            MinimumQuantity = 2m,
            IsOutOfStock = true,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.InventoryItems.Add(item);
        await db.SaveChangesAsync();

        var result = await handler.Handle(new UseInventoryRequest(item.Id, 1m, "Test"), CancellationToken.None);

        result.GetType().Name.Should().StartWith("BadRequest");
        var valueProp = result.GetType().GetProperty("Value")!;
        var payload = valueProp.GetValue(result)!;
        var errProp = payload.GetType().GetProperty("error")!;
        errProp.GetValue(payload)!.ToString().Should().Contain("out of stock");
    }

    [Fact]
    public async Task GivenInsufficientQuantity_WhenUsingInventory_ThenReturnsBadRequest()
    {
        await using var db = CreateDbContext();
        var handler = CreateHandler(db);

        var item = new InventoryItem
        {
            Id = Guid.NewGuid(),
            Name = "Beef",
            Unit = "kg",
            CurrentQuantity = 1m,
            MinimumQuantity = 0.5m,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.InventoryItems.Add(item);
        await db.SaveChangesAsync();

        var result = await handler.Handle(new UseInventoryRequest(item.Id, 2m, "Big order"), CancellationToken.None);

        result.GetType().Name.Should().StartWith("BadRequest");
        var valueProp = result.GetType().GetProperty("Value")!;
        var payload = valueProp.GetValue(result)!;
        var errProp = payload.GetType().GetProperty("error")!;
        errProp.GetValue(payload)!.ToString().Should().Contain("Insufficient quantity");
        var requestedProp = payload.GetType().GetProperty("requested")!;
        requestedProp.GetValue(payload).Should().Be(2m);
    }

    [Fact]
    public async Task GivenInvalidRequest_WhenUsingInventory_ThenReturnsValidationErrors()
    {
        await using var db = CreateDbContext();
        var handler = CreateHandler(db);

        var result = await handler.Handle(new UseInventoryRequest(Guid.Empty, 0m, new string('x', 501)), CancellationToken.None);

        result.GetType().Name.Should().StartWith("BadRequest");
        var valueProp = result.GetType().GetProperty("Value")!;
        var payload = valueProp.GetValue(result)!;
        var errorsProp = payload.GetType().GetProperty("errors")!;
        var errors = errorsProp.GetValue(payload)! as System.Collections.IDictionary;
        errors.Should().NotBeNull();
        errors!.Contains("InventoryItemId").Should().BeTrue();
        errors!.Contains("Quantity").Should().BeTrue();
        errors!.Contains("Reason").Should().BeTrue();
    }
}
