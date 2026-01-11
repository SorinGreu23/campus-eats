using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Inventory.GetById;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Tests.Features.Inventory.GetById;

public class GetInventoryItemHandlerTests
{
    private static CampusDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CampusDbContext(options);
    }

    [Fact]
    public async Task GivenExistingItemWithTransactions_WhenGetting_ThenReturnsOkWithDtoAndTop10Transactions()
    {
        await using var db = CreateDbContext();
        var handler = new GetInventoryItemHandler(db);

        var item = new InventoryItem
        {
            Id = Guid.NewGuid(),
            Name = "Tomatoes",
            Unit = "kg",
            CurrentQuantity = 3,
            MinimumQuantity = 5,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        db.InventoryItems.Add(item);

        // Seed 12 transactions; handler should return the latest 10
        var transactions = Enumerable.Range(0, 12).Select(i => new InventoryTransaction
        {
            Id = Guid.NewGuid(),
            InventoryItemId = item.Id,
            TransactionType = i % 2 == 0 ? "Add" : "Use",
            Quantity = 1 + i,
            Reason = i % 2 == 0 ? "Restock" : "Order",
            PerformedBy = "tester",
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(i)
        }).ToList();

        db.InventoryTransactions.AddRange(transactions);
        await db.SaveChangesAsync();

        var request = new GetInventoryItemRequest(item.Id);
        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().BeAssignableTo<IResult>();

        var valueProp = result.GetType().GetProperty("Value");
        valueProp.Should().NotBeNull();
        var value = valueProp!.GetValue(result);
        value.Should().NotBeNull();

        var itemProp = value!.GetType().GetProperty("Item");
        var txProp = value!.GetType().GetProperty("RecentTransactions");
        itemProp.Should().NotBeNull();
        txProp.Should().NotBeNull();

        var itemDto = itemProp!.GetValue(value)!;
        var idProp = itemDto.GetType().GetProperty("Id")!;
        var nameProp = itemDto.GetType().GetProperty("Name")!;
        var unitProp = itemDto.GetType().GetProperty("Unit")!;
        var currentQProp = itemDto.GetType().GetProperty("CurrentQuantity")!;
        var minQProp = itemDto.GetType().GetProperty("MinimumQuantity")!;
        var lowStockProp = itemDto.GetType().GetProperty("IsLowStock")!;
        var outOfStockProp = itemDto.GetType().GetProperty("IsOutOfStock")!;

        idProp.GetValue(itemDto).Should().Be(item.Id);
        nameProp.GetValue(itemDto).Should().Be("Tomatoes");
        unitProp.GetValue(itemDto).Should().Be("kg");
        currentQProp.GetValue(itemDto).Should().Be(3m);
        minQProp.GetValue(itemDto).Should().Be(5m);
        lowStockProp.GetValue(itemDto).Should().Be(true);
        outOfStockProp.GetValue(itemDto).Should().Be(false);

        var recent = txProp!.GetValue(value) as IEnumerable<object>;
        recent.Should().NotBeNull();
        var list = recent!.ToList();
        list.Count.Should().Be(10);

        // Verify ordering: first should have the latest CreatedAt
        var createdAtProp = list[0].GetType().GetProperty("CreatedAt")!;
        var firstCreatedAt = (DateTimeOffset)createdAtProp.GetValue(list[0])!;
        firstCreatedAt.Should().Be(transactions.Max(t => t.CreatedAt));
    }

    [Fact]
    public async Task GivenOutOfStockItem_WhenGetting_ThenReturnsOkWithOutOfStockTrueAndNoTransactions()
    {
        await using var db = CreateDbContext();
        var handler = new GetInventoryItemHandler(db);

        var item = new InventoryItem
        {
            Id = Guid.NewGuid(),
            Name = "Lettuce",
            Unit = "pcs",
            CurrentQuantity = 0,
            MinimumQuantity = 2,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.InventoryItems.Add(item);
        await db.SaveChangesAsync();

        var request = new GetInventoryItemRequest(item.Id);
        var result = await handler.Handle(request, CancellationToken.None);

        var valueProp = result.GetType().GetProperty("Value")!;
        var value = valueProp.GetValue(result)!;
        var itemProp = value.GetType().GetProperty("Item")!;
        var txProp = value.GetType().GetProperty("RecentTransactions")!;

        var itemDto = itemProp.GetValue(value)!;
        var outOfStockProp = itemDto.GetType().GetProperty("IsOutOfStock")!;
        outOfStockProp.GetValue(itemDto).Should().Be(true);

        var recent = txProp.GetValue(value) as IEnumerable<object>;
        recent!.Count().Should().Be(0);
    }

    [Fact]
    public async Task GivenMissingItem_WhenGetting_ThenReturnsNotFoundWithMessage()
    {
        await using var db = CreateDbContext();
        var handler = new GetInventoryItemHandler(db);

        var id = Guid.NewGuid();
        var request = new GetInventoryItemRequest(id);
        var result = await handler.Handle(request, CancellationToken.None);

        result.GetType().Name.Should().StartWith("NotFound");
        var valueProp = result.GetType().GetProperty("Value")!;
        var message = valueProp.GetValue(result) as string;
        message.Should().Be($"Inventory item with ID '{id}' was not found.");
    }
}
