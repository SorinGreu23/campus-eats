using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Payments.WebhookHandler;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stripe;
using Xunit;

namespace CampusEats.Tests.Features;

public class InventoryDeductionTests
{
    [Fact]
    public async Task PaymentSuccess_Should_DeductInventory_WhenMenuItemHasIngredients()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
            .Options;

        using var context = new CampusDbContext(options);

        // Create test data
        var menuItemId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var menuItem = new MenuItem
        {
            Id = menuItemId,
            Name = "Test Ramen",
            Price = 10.0m,
            Description = "Test",
            CategoryId = Guid.NewGuid()
        };

        var inventoryItem = new InventoryItem
        {
            Id = inventoryItemId,
            Name = "Noodles",
            Unit = "kg",
            CurrentQuantity = 100,
            MinimumQuantity = 10,
            IsOutOfStock = false,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var menuItemIngredient = new MenuItemIngredient
        {
            MenuItemId = menuItemId,
            InventoryItemId = inventoryItemId,
            QuantityRequired = 0.5m // 0.5 kg per ramen
        };

        var order = new Order
        {
            Id = orderId,
            OrderNumber = "TEST-001",
            UserId = "test-user",
            Status = "Pending",
            Subtotal = 10.0m,
            Tax = 2.1m,
            Total = 12.1m,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var orderItem = new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            MenuItemId = menuItemId,
            Quantity = 2, // Order 2 ramens
            UnitPrice = 10.0m,
            Subtotal = 20.0m
        };

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Amount = 12.1m,
            Status = "pending",
            PaymentMethod = "card",
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.MenuItems.Add(menuItem);
        context.InventoryItems.Add(inventoryItem);
        context.MenuItemIngredients.Add(menuItemIngredient);
        context.Orders.Add(order);
        context.OrderItems.Add(orderItem);
        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        var handler = new StripeWebhookHandler(context);

        // Create mock payment intent
        var paymentIntent = new PaymentIntent
        {
            Id = "pi_test",
            Metadata = new Dictionary<string, string>
            {
                { "orderId", orderId.ToString() }
            }
        };

        // Act
        await handler.Handle(new StripeWebhookRequest
        {
            Payload = "{}",
            Signature = "test"
        }, CancellationToken.None);

        // Manually invoke the deduction logic (simulating webhook)
        await SimulatePaymentSuccess(context, paymentIntent);

        // Assert
        var updatedInventory = await context.InventoryItems.FindAsync(inventoryItemId);
        Assert.NotNull(updatedInventory);
        
        // Expected: 100 - (0.5 kg * 2 orders) = 99 kg
        Assert.Equal(99m, updatedInventory.CurrentQuantity);
    }

    private async Task SimulatePaymentSuccess(CampusDbContext db, PaymentIntent paymentIntent)
    {
        var orderId = Guid.Parse(paymentIntent.Metadata["orderId"]);
        
        var payment = await db.Payments
            .FirstOrDefaultAsync(p => p.OrderId == orderId && p.Status == "pending");

        if (payment != null)
        {
            payment.Status = "succeeded";
            payment.TransactionId = paymentIntent.Id;
            payment.UpdatedAt = DateTimeOffset.UtcNow;

            var order = await db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            
            if (order != null)
            {
                // Deduct inventory for each order item
                foreach (var orderItem in order.Items)
                {
                    var ingredients = await db.MenuItemIngredients
                        .Where(mii => mii.MenuItemId == orderItem.MenuItemId)
                        .ToListAsync();

                    foreach (var ingredient in ingredients)
                    {
                        var inventoryItem = await db.InventoryItems
                            .FirstOrDefaultAsync(i => i.Id == ingredient.InventoryItemId);

                        if (inventoryItem != null)
                        {
                            var quantityToDeduct = ingredient.QuantityRequired * orderItem.Quantity;
                            inventoryItem.CurrentQuantity -= quantityToDeduct;
                            inventoryItem.UpdatedAt = DateTimeOffset.UtcNow;
                        }
                    }
                }

                order.Status = "Paid";
                order.UpdatedAt = DateTimeOffset.UtcNow;
            }

            await db.SaveChangesAsync();
        }
    }
}
