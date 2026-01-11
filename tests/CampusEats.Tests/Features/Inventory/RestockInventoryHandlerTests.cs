using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Inventory.Restock;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CampusEats.Tests.Features.Inventory;

public class RestockInventoryHandlerTests
{
    private CampusDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CampusDbContext(options);
    }

    private static (object? value, string? location) ReadResultPayload(IResult result)
    {
        var type = result.GetType();
        var valueProp = type.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);
        var locationProp = type.GetProperty("Location", BindingFlags.Instance | BindingFlags.Public);
        var value = valueProp?.GetValue(result);
        var location = locationProp?.GetValue(result) as string;
        return (value, location);
    }

    [Fact]
    public async Task Restock_Succeeds_AddsTransactionAndUpdatesQuantity()
    {
        using var context = CreateContext();
        var validator = new RestockInventoryValidator();
        var handler = new RestockInventoryHandler(context, validator);

        var item = new InventoryItem
        {
            Id = Guid.NewGuid(),
            Name = "Tomatoes",
            Unit = "kg",
            CurrentQuantity = 5,
            MinimumQuantity = 1,
            IsOutOfStock = false,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        context.InventoryItems.Add(item);
        await context.SaveChangesAsync();

        var request = new RestockInventoryRequest(item.Id, 3m, "Delivery");
        var result = await handler.Handle(request, CancellationToken.None);

        var (value, location) = ReadResultPayload(result);
        location.Should().NotBeNull();
        location!.Should().StartWith("/api/inventory/transactions/");

        // Anonymous payload; assert via reflection on properties
        var payloadType = value!.GetType();
        var id = (Guid)payloadType.GetProperty("Id")!.GetValue(value)!;
        var inventoryItemId = (Guid)payloadType.GetProperty("InventoryItemId")!.GetValue(value)!;
        var itemName = (string)payloadType.GetProperty("ItemName")!.GetValue(value)!;
        var transactionType = (string)payloadType.GetProperty("TransactionType")!.GetValue(value)!;
        var quantity = (decimal)payloadType.GetProperty("Quantity")!.GetValue(value)!;
        var newQuantity = (decimal)payloadType.GetProperty("NewQuantity")!.GetValue(value)!;
        var reason = (string)payloadType.GetProperty("Reason")!.GetValue(value)!;
        var performedBy = (string)payloadType.GetProperty("PerformedBy")!.GetValue(value)!;

        inventoryItemId.Should().Be(item.Id);
        itemName.Should().Be("Tomatoes");
        transactionType.Should().Be("Restock");
        quantity.Should().Be(3m);
        newQuantity.Should().Be(8m);
        reason.Should().Be("Delivery");
        performedBy.Should().Be("chef");

        var transaction = await context.InventoryTransactions.FirstOrDefaultAsync(t => t.Id == id);
        transaction.Should().NotBeNull();
        transaction!.TransactionType.Should().Be("Restock");
        transaction.Quantity.Should().Be(3m);

        var updatedItem = await context.InventoryItems.FindAsync(item.Id);
        updatedItem!.CurrentQuantity.Should().Be(8m);
        updatedItem.IsOutOfStock.Should().BeFalse();
    }

    [Fact]
    public async Task Restock_NotFound_ReturnsNotFound()
    {
        using var context = CreateContext();
        var validator = new RestockInventoryValidator();
        var handler = new RestockInventoryHandler(context, validator);

        var request = new RestockInventoryRequest(Guid.NewGuid(), 2m, "Delivery");
        var result = await handler.Handle(request, CancellationToken.None);

        // Result should be NotFound; Value contains message string
        var (value, _) = ReadResultPayload(result);
        value.Should().BeOfType<string>();
        value!.ToString().Should().Contain("was not found");
    }

    [Fact]
    public async Task Restock_InvalidQuantity_ReturnsValidationErrors()
    {
        using var context = CreateContext();
        var validator = new RestockInventoryValidator();
        var handler = new RestockInventoryHandler(context, validator);

        var itemId = Guid.NewGuid();
        var request = new RestockInventoryRequest(itemId, 0m, "Bad");
        var result = await handler.Handle(request, CancellationToken.None);

        var (value, _) = ReadResultPayload(result);
        var errorsProp = value!.GetType().GetProperty("errors")!;
        var errors = errorsProp.GetValue(value);
        errors.Should().NotBeNull();
        // Ensure Quantity key exists
        var qtyErrors = (errors as System.Collections.IDictionary)!["Quantity"] as string[];
        qtyErrors.Should().NotBeNull();
        qtyErrors!.First().Should().Contain("greater than 0");
    }

    [Fact]
    public async Task Restock_InvalidReasonLength_ReturnsValidationErrors()
    {
        using var context = CreateContext();
        var validator = new RestockInventoryValidator();
        var handler = new RestockInventoryHandler(context, validator);

        var longReason = new string('x', 501);
        var request = new RestockInventoryRequest(Guid.NewGuid(), 1m, longReason);
        var result = await handler.Handle(request, CancellationToken.None);

        var (value, _) = ReadResultPayload(result);
        var errors = value!.GetType().GetProperty("errors")!.GetValue(value) as System.Collections.IDictionary;
        errors.Should().NotBeNull();
        var reasonErrors = errors!["Reason"] as string[];
        reasonErrors.Should().NotBeNull();
        reasonErrors!.First().Should().Contain("must not exceed 500 characters");
    }

    [Fact]
    public async Task Restock_EmptyItemId_ReturnsValidationErrors()
    {
        using var context = CreateContext();
        var validator = new RestockInventoryValidator();
        var handler = new RestockInventoryHandler(context, validator);

        var request = new RestockInventoryRequest(Guid.Empty, 1m, "Reason");
        var result = await handler.Handle(request, CancellationToken.None);

        var (value, _) = ReadResultPayload(result);
        var errors = value!.GetType().GetProperty("errors")!.GetValue(value) as System.Collections.IDictionary;
        errors.Should().NotBeNull();
        var idErrors = errors!["InventoryItemId"] as string[];
        idErrors.Should().NotBeNull();
        idErrors!.First().Should().Contain("required");
    }
}
