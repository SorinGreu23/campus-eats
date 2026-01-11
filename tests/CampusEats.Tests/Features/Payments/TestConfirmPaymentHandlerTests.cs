using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Payments.TestConfirm;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Tests.Features.Payments.TestConfirm;

public class TestConfirmPaymentHandlerTests
{
    private static CampusDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CampusDbContext(options);
    }

    private static HttpContextAccessor CreateAccessor(string? userId = null, bool authenticated = true)
    {
        var ctx = new DefaultHttpContext();
        ClaimsIdentity identity = authenticated
            ? new ClaimsIdentity(
                userId != null ? new[] { new Claim(ClaimTypes.NameIdentifier, userId) } : Array.Empty<Claim>(),
                "TestAuth")
            : new ClaimsIdentity();
        ctx.User = new ClaimsPrincipal(identity);
        return new HttpContextAccessor { HttpContext = ctx };
    }

    [Fact]
    public async Task WhenNotAuthenticated_ReturnsUnauthorized()
    {
        await using var db = CreateDb();
        var accessor = CreateAccessor(authenticated: false);
        var handler = new TestConfirmPaymentHandler(db, accessor);

        var result = await handler.Handle(new TestConfirmPaymentRequest { PaymentId = Guid.NewGuid() }, CancellationToken.None);
        result.GetType().Name.Should().StartWith("Unauthorized");
    }

    [Fact]
    public async Task WhenMissingNameIdentifier_ReturnsUnauthorized()
    {
        await using var db = CreateDb();
        var ctx = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity("TestAuth")) };
        var accessor = new HttpContextAccessor { HttpContext = ctx };
        var handler = new TestConfirmPaymentHandler(db, accessor);

        var result = await handler.Handle(new TestConfirmPaymentRequest { PaymentId = Guid.NewGuid() }, CancellationToken.None);
        result.GetType().Name.Should().StartWith("Unauthorized");
    }

    [Fact]
    public async Task WhenPaymentNotFound_ReturnsNotFound()
    {
        await using var db = CreateDb();
        var accessor = CreateAccessor(userId: "user1");
        var handler = new TestConfirmPaymentHandler(db, accessor);

        var id = Guid.NewGuid();
        var result = await handler.Handle(new TestConfirmPaymentRequest { PaymentId = id }, CancellationToken.None);
        result.GetType().Name.Should().StartWith("NotFound");
        var valueProp = result.GetType().GetProperty("Value")!;
        var payload = valueProp.GetValue(result)!;
        payload.GetType().GetProperty("error")!.GetValue(payload)!.ToString().Should().Be("Payment not found.");
    }

    [Fact]
    public async Task WhenUserDoesNotOwnPayment_ReturnsForbid()
    {
        await using var db = CreateDb();
        var otherUser = "other-user";
        var accessor = CreateAccessor(userId: otherUser);
        var handler = new TestConfirmPaymentHandler(db, accessor);

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            UserId = "owner",
            Amount = 10m,
            Status = "requires_confirmation",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.Payments.Add(payment);
        await db.SaveChangesAsync();

        var result = await handler.Handle(new TestConfirmPaymentRequest { PaymentId = payment.Id }, CancellationToken.None);
        result.GetType().Name.Should().Be("ForbidHttpResult");
    }

    [Fact]
    public async Task WhenValidOwnedPayment_ConfirmsAndDeductsInventory()
    {
        await using var db = CreateDb();
        var userId = "user-123";
        var accessor = CreateAccessor(userId: userId);
        var handler = new TestConfirmPaymentHandler(db, accessor);

        // Seed inventory and menu ingredients
        var invTomato = new InventoryItem { Id = Guid.NewGuid(), Name = "Tomato", Unit = "kg", CurrentQuantity = 10m, MinimumQuantity = 1m, UpdatedAt = DateTimeOffset.UtcNow };
        var menuItem = new MenuItem { Id = Guid.NewGuid(), Name = "Salad", Price = 5m, IsAvailable = true, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow };
        var ingredient = new MenuItemIngredient { MenuItemId = menuItem.Id, InventoryItemId = invTomato.Id, QuantityRequired = 0.5m };
        db.InventoryItems.Add(invTomato);
        db.MenuItems.Add(menuItem);
        db.MenuItemIngredients.Add(ingredient);

        // Seed order with 2 quantity of menu item
        var order = new Order { Id = Guid.NewGuid(), OrderNumber = "ORD-1", UserId = userId, Status = "Pending", CreatedAt = DateTimeOffset.UtcNow };
        var orderItem = new OrderItem { Id = Guid.NewGuid(), OrderId = order.Id, MenuItemId = menuItem.Id, Quantity = 2, UnitPrice = 5m, Subtotal = 10m };
        order.Items.Add(orderItem);
        db.Orders.Add(order);
        db.OrderItems.Add(orderItem);

        var payment = new Payment { Id = Guid.NewGuid(), UserId = userId, OrderId = order.Id, Amount = 10m, Status = "requires_confirmation", CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow };
        db.Payments.Add(payment);
        await db.SaveChangesAsync();

        var result = await handler.Handle(new TestConfirmPaymentRequest { PaymentId = payment.Id }, CancellationToken.None);

        result.GetType().Name.Should().StartWith("Ok");
        var valueProp = result.GetType().GetProperty("Value")!;
        var payload = valueProp.GetValue(result)!;
        payload.GetType().GetProperty("status")!.GetValue(payload)!.ToString().Should().Be("succeeded");
        var txId = payload.GetType().GetProperty("transactionId")!.GetValue(payload)!.ToString();
        txId.Should().NotBeNullOrEmpty();

        var savedPayment = await db.Payments.FirstAsync(p => p.Id == payment.Id);
        savedPayment.Status.Should().Be("succeeded");
        savedPayment.TransactionId.Should().Be(txId);

        var savedOrder = await db.Orders.Include(o => o.Items).FirstAsync(o => o.Id == order.Id);
        savedOrder.Status.Should().Be("Pending");

        var savedInv = await db.InventoryItems.FirstAsync(i => i.Id == invTomato.Id);
        savedInv.CurrentQuantity.Should().Be(10m - (0.5m * 2));
    }
}
