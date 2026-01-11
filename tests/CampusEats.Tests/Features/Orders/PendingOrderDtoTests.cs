using CampusEats.Api.Features.Kitchen;
using FluentAssertions;

namespace CampusEats.Tests.Features.Orders;

public class PendingOrderDtoTests
{
    [Fact]
    public void PendingOrderDto_CanBeCreated()
    {
        // Arrange & Act
        var dto = new PendingOrderDto
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            Status = "Paid",
            Total = 100.50m,
            SpecialInstructions = "No onions",
            PickupTime = DateTimeOffset.UtcNow.AddHours(1),
            CreatedAt = DateTimeOffset.UtcNow,
            Items = new List<PendingOrderItemDto>()
        };

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().NotBeEmpty();
        dto.OrderNumber.Should().Be("ORD-001");
        dto.Status.Should().Be("Paid");
        dto.Total.Should().Be(100.50m);
        dto.SpecialInstructions.Should().Be("No onions");
        dto.PickupTime.Should().NotBeNull();
        dto.CreatedAt.Should().NotBeNull();
        dto.Items.Should().NotBeNull();
        dto.Items.Should().BeEmpty();
    }

    [Fact]
    public void PendingOrderDto_DefaultsToEmptyItemsList()
    {
        // Arrange & Act
        var dto = new PendingOrderDto();

        // Assert
        dto.Items.Should().NotBeNull();
        dto.Items.Should().BeEmpty();
    }

    [Fact]
    public void PendingOrderDto_CanHaveMultipleItems()
    {
        // Arrange
        var item1 = new PendingOrderItemDto
        {
            Id = Guid.NewGuid(),
            MenuItemName = "Burger",
            MenuItemImageUrl = "https://example.com/burger.jpg",
            Quantity = 2,
            UnitPrice = 10.00m,
            Subtotal = 20.00m,
            SpecialInstructions = "Extra cheese"
        };

        var item2 = new PendingOrderItemDto
        {
            Id = Guid.NewGuid(),
            MenuItemName = "Fries",
            MenuItemImageUrl = "https://example.com/fries.jpg",
            Quantity = 1,
            UnitPrice = 5.00m,
            Subtotal = 5.00m,
            SpecialInstructions = null
        };

        // Act
        var dto = new PendingOrderDto
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-002",
            Status = "Preparing",
            Total = 25.00m,
            Items = new List<PendingOrderItemDto> { item1, item2 }
        };

        // Assert
        dto.Items.Should().HaveCount(2);
        dto.Items.First().MenuItemName.Should().Be("Burger");
        dto.Items.Last().MenuItemName.Should().Be("Fries");
    }

    [Fact]
    public void PendingOrderItemDto_CanBeCreated()
    {
        // Arrange & Act
        var dto = new PendingOrderItemDto
        {
            Id = Guid.NewGuid(),
            MenuItemName = "Pizza",
            MenuItemImageUrl = "https://example.com/pizza.jpg",
            Quantity = 3,
            UnitPrice = 12.99m,
            Subtotal = 38.97m,
            SpecialInstructions = "No mushrooms"
        };

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().NotBeEmpty();
        dto.MenuItemName.Should().Be("Pizza");
        dto.MenuItemImageUrl.Should().Be("https://example.com/pizza.jpg");
        dto.Quantity.Should().Be(3);
        dto.UnitPrice.Should().Be(12.99m);
        dto.Subtotal.Should().Be(38.97m);
        dto.SpecialInstructions.Should().Be("No mushrooms");
    }

    [Fact]
    public void PendingOrderItemDto_AllowsNullSpecialInstructions()
    {
        // Arrange & Act
        var dto = new PendingOrderItemDto
        {
            Id = Guid.NewGuid(),
            MenuItemName = "Salad",
            Quantity = 1,
            UnitPrice = 8.00m,
            Subtotal = 8.00m,
            SpecialInstructions = null
        };

        // Assert
        dto.SpecialInstructions.Should().BeNull();
    }

    [Fact]
    public void PendingOrderDto_AllowsNullOptionalFields()
    {
        // Arrange & Act
        var dto = new PendingOrderDto
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-003",
            Status = "Ready",
            Total = 50.00m,
            SpecialInstructions = null,
            PickupTime = null,
            CreatedAt = null
        };

        // Assert
        dto.SpecialInstructions.Should().BeNull();
        dto.PickupTime.Should().BeNull();
        dto.CreatedAt.Should().BeNull();
    }

    [Fact]
    public void PendingOrderDto_CanCalculateTotalFromItems()
    {
        // Arrange
        var items = new List<PendingOrderItemDto>
        {
            new() { Id = Guid.NewGuid(), MenuItemName = "Item1", Quantity = 2, UnitPrice = 10m, Subtotal = 20m },
            new() { Id = Guid.NewGuid(), MenuItemName = "Item2", Quantity = 1, UnitPrice = 15m, Subtotal = 15m },
            new() { Id = Guid.NewGuid(), MenuItemName = "Item3", Quantity = 3, UnitPrice = 5m, Subtotal = 15m }
        };

        var expectedTotal = items.Sum(i => i.Subtotal);

        // Act
        var dto = new PendingOrderDto
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-004",
            Status = "Paid",
            Total = expectedTotal,
            Items = items
        };

        // Assert
        dto.Total.Should().Be(50m);
        dto.Items.Sum(i => i.Subtotal).Should().Be(dto.Total);
    }

    [Fact]
    public void PendingOrderItemDto_SubtotalMatchesQuantityTimesPrice()
    {
        // Arrange
        var quantity = 4;
        var unitPrice = 7.50m;
        var expectedSubtotal = quantity * unitPrice;

        // Act
        var dto = new PendingOrderItemDto
        {
            Id = Guid.NewGuid(),
            MenuItemName = "Sandwich",
            Quantity = quantity,
            UnitPrice = unitPrice,
            Subtotal = expectedSubtotal
        };

        // Assert
        dto.Subtotal.Should().Be(30.00m);
        dto.Subtotal.Should().Be(dto.Quantity * dto.UnitPrice);
    }
}
