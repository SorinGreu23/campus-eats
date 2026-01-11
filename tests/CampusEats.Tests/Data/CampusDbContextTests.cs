using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Tests.Data;

public class CampusDbContextTests
{
    private readonly DbContextOptions<CampusDbContext> _options;

    public CampusDbContextTests()
    {
        _options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GivenMenuItem_WhenAdding_ThenSetsCreatedAt()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);
        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = "Test Burger",
            Description = "A delicious test burger",
            Price = 12.99m,
            IsAvailable = true
        };

        // Act
        context.MenuItems.Add(menuItem);
        await context.SaveChangesAsync();

        // Assert
        menuItem.CreatedAt.Should().NotBe(default);
        menuItem.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GivenMenuItem_WhenUpdating_ThenSetsUpdatedAt()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);
        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = "Test Pizza",
            Description = "Original description",
            Price = 15.99m,
            IsAvailable = true
        };

        context.MenuItems.Add(menuItem);
        await context.SaveChangesAsync();

        var originalCreatedAt = menuItem.CreatedAt;
        var originalUpdatedAt = menuItem.UpdatedAt;

        // Wait a moment to ensure timestamp difference
        await Task.Delay(100);

        // Act
        menuItem.Description = "Updated description";
        menuItem.Price = 17.99m;
        await context.SaveChangesAsync();

        // Assert
        menuItem.UpdatedAt.Should().NotBe(originalUpdatedAt);
        menuItem.UpdatedAt.Should().BeAfter(originalCreatedAt);
        menuItem.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        menuItem.CreatedAt.Should().Be(originalCreatedAt); // CreatedAt should not change
    }

    [Fact]
    public async Task GivenCascadeDelete_WhenDeletingCategory_ThenDeletesMenuItems()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);
        var categoryId = Guid.NewGuid();
        var category = new Category
        {
            Id = categoryId,
            Name = "Test Category",
            DisplayOrder = 1,
            IsActive = true
        };

        var menuItem1 = new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = "Item 1",
            Price = 10.00m,
            CategoryId = categoryId,
            IsAvailable = true
        };

        var menuItem2 = new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = "Item 2",
            Price = 12.00m,
            CategoryId = categoryId,
            IsAvailable = true
        };

        context.Categories.Add(category);
        context.MenuItems.AddRange(menuItem1, menuItem2);
        await context.SaveChangesAsync();

        // Verify items were added
        var itemsBeforeDelete = await context.MenuItems.CountAsync(m => m.CategoryId == categoryId);
        itemsBeforeDelete.Should().Be(2);

        // Act - Delete category
        context.Categories.Remove(category);
        await context.SaveChangesAsync();

        // Assert - Menu items should have CategoryId set to null (SetNull behavior)
        var item1AfterDelete = await context.MenuItems.FindAsync(menuItem1.Id);
        var item2AfterDelete = await context.MenuItems.FindAsync(menuItem2.Id);

        item1AfterDelete.Should().NotBeNull();
        item1AfterDelete!.CategoryId.Should().BeNull();
        
        item2AfterDelete.Should().NotBeNull();
        item2AfterDelete!.CategoryId.Should().BeNull();
    }

    [Fact]
    public async Task GivenOrder_WhenAdding_ThenSetsCreatedAt()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            UserId = "test-user",
            Status = "Pending",
            Subtotal = 100m,
            Tax = 21m,
            Discount = 0m,
            Total = 121m
        };

        // Act
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Assert
        order.CreatedAt.Should().NotBe(default);
        order.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GivenOrderWithItems_WhenDeletingOrder_ThenDeletesOrderItems()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);
        var menuItemId = Guid.NewGuid();
        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Test Item",
            Price = 10.00m,
            IsAvailable = true
        };

        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-002",
            UserId = "test-user",
            Status = "Pending",
            Subtotal = 20m,
            Tax = 4.2m,
            Discount = 0m,
            Total = 24.2m,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var orderItem = new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            MenuItemId = menuItemId,
            Quantity = 2,
            UnitPrice = 10.00m,
            Subtotal = 20.00m
        };

        context.MenuItems.Add(menuItem);
        context.Orders.Add(order);
        context.OrderItems.Add(orderItem);
        await context.SaveChangesAsync();

        var orderItemId = orderItem.Id;

        // Act - Delete order
        context.Orders.Remove(order);
        await context.SaveChangesAsync();

        // Assert - Order item should be deleted (Cascade behavior)
        var deletedOrderItem = await context.OrderItems.FindAsync(orderItemId);
        deletedOrderItem.Should().BeNull();

        // Menu item should still exist
        var existingMenuItem = await context.MenuItems.FindAsync(menuItemId);
        existingMenuItem.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenMenuItemWithAllergens_WhenDeletingMenuItem_ThenDeletesJoinTableRecords()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);
        var menuItemId = Guid.NewGuid();
        var allergenId = Guid.NewGuid();

        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Test Food",
            Price = 8.00m,
            IsAvailable = true
        };

        var allergen = new Allergen
        {
            Id = allergenId,
            Name = "Peanuts",
            Description = "Contains peanuts"
        };

        var menuItemAllergen = new MenuItemAllergen
        {
            MenuItemId = menuItemId,
            AllergenId = allergenId
        };

        context.MenuItems.Add(menuItem);
        context.Allergens.Add(allergen);
        context.MenuItemAllergens.Add(menuItemAllergen);
        await context.SaveChangesAsync();

        // Act - Delete menu item
        context.MenuItems.Remove(menuItem);
        await context.SaveChangesAsync();

        // Assert - Join table record should be deleted (Cascade)
        var deletedJoinRecord = await context.MenuItemAllergens
            .FirstOrDefaultAsync(ma => ma.MenuItemId == menuItemId && ma.AllergenId == allergenId);
        deletedJoinRecord.Should().BeNull();

        // Allergen itself should still exist
        var existingAllergen = await context.Allergens.FindAsync(allergenId);
        existingAllergen.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenLoyaltyAccount_WhenAdding_ThenPersistsCorrectly()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);
        var loyaltyAccount = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = "test-user-123",
            PointsBalance = 500,
            LifetimePoints = 1500,
            Tier = "Silver"
        };

        // Act
        context.LoyaltyAccounts.Add(loyaltyAccount);
        await context.SaveChangesAsync();

        // Assert
        var savedAccount = await context.LoyaltyAccounts
            .FirstOrDefaultAsync(la => la.UserId == "test-user-123");
        
        savedAccount.Should().NotBeNull();
        savedAccount!.PointsBalance.Should().Be(500);
        savedAccount.LifetimePoints.Should().Be(1500);
        savedAccount.Tier.Should().Be("Silver");
    }

    [Fact]
    public async Task GivenInventoryItem_WhenUpdating_ThenPersistsChanges()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);
        var inventoryItem = new InventoryItem
        {
            Id = Guid.NewGuid(),
            Name = "Tomatoes",
            Unit = "kg",
            CurrentQuantity = 10.0m,
            MinimumQuantity = 2.0m,
            IsOutOfStock = false,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        context.InventoryItems.Add(inventoryItem);
        await context.SaveChangesAsync();

        // Act - Update quantity
        inventoryItem.CurrentQuantity = 5.0m;
        inventoryItem.UpdatedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync();

        // Assert
        var updatedItem = await context.InventoryItems.FindAsync(inventoryItem.Id);
        updatedItem.Should().NotBeNull();
        updatedItem!.CurrentQuantity.Should().Be(5.0m);
    }

    [Fact]
    public async Task GivenPayment_WhenOrderIsDeleted_ThenPaymentOrderIdSetToNull()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-003",
            UserId = "test-user",
            Status = "Paid",
            Subtotal = 50m,
            Tax = 10.5m,
            Discount = 0m,
            Total = 60.5m,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            UserId = "test-user",
            Amount = 60.5m,
            Status = "succeeded",
            PaymentMethod = "stripe",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        context.Orders.Add(order);
        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        var paymentId = payment.Id;

        // Act - Delete order
        context.Orders.Remove(order);
        await context.SaveChangesAsync();

        // Assert - Payment should still exist but OrderId set to null (SetNull behavior)
        var existingPayment = await context.Payments.FindAsync(paymentId);
        existingPayment.Should().NotBeNull();
        existingPayment!.OrderId.Should().BeNull();
    }
}
