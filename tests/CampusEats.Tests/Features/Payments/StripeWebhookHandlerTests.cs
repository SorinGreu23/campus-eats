using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Payments.WebhookHandler;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace CampusEats.Tests.Features.Payments;

public class StripeWebhookHandlerTests
{
    private static CampusDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CampusDbContext(options);
    }

    private static IConfiguration CreateConfig(string? secret = null)
    {
        var data = new Dictionary<string, string?>();
        if (secret != null)
        {
            data["Stripe:WebhookSecret"] = secret;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
    }

    private static string CreateSignatureHeader(string payload, string secret, long timestamp)
    {
        var toSign = $"{timestamp}.{payload}";
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(toSign);

        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(payloadBytes);
        var signature = string.Concat(hash.Select(b => b.ToString("x2")));

        return $"t={timestamp},v1={signature}";
    }

    [Fact]
    public async Task GivenMissingWebhookSecret_WhenHandlingWebhook_ThenReturnsProblem()
    {
        await using var context = CreateContext();
        var config = CreateConfig(null);
        var handler = new StripeWebhookHandler(context, config);

        var result = await handler.Handle(new StripeWebhookRequest
        {
            Payload = "{}",
            Signature = string.Empty
        }, CancellationToken.None);

        result.Should().BeOfType<ProblemHttpResult>();
    }

    [Fact]
    public async Task GivenInvalidSignature_WhenHandlingWebhook_ThenReturnsBadRequest()
    {
        await using var context = CreateContext();
        var config = CreateConfig("whsec_test");
        var handler = new StripeWebhookHandler(context, config);

        var result = await handler.Handle(new StripeWebhookRequest
        {
            Payload = "{}",
            Signature = string.Empty
        }, CancellationToken.None);

        var badRequest = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task GivenSucceededPaymentIntent_WhenSignatureValid_ThenMarksPaymentSucceededAndDeductsInventory()
    {
        await using var context = CreateContext();
        var secret = "whsec_test";
        var config = CreateConfig(secret);
        var handler = new StripeWebhookHandler(context, config);

        var orderId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();

        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-123",
            UserId = "user-1",
            Status = "Pending",
            OrderType = "Pickup",
            Subtotal = 20m,
            Tax = 2m,
            Total = 22m,
            CreatedAt = DateTimeOffset.UtcNow,
            Items =
            {
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    MenuItemId = menuItemId,
                    Quantity = 2,
                    UnitPrice = 10m,
                    Subtotal = 20m
                }
            }
        };

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Amount = 22m,
            Status = "pending",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var inventoryItem = new InventoryItem
        {
            Id = ingredientId,
            Name = "Chicken",
            Unit = "kg",
            CurrentQuantity = 10m,
            MinimumQuantity = 1m,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var menuItemIngredient = new MenuItemIngredient
        {
            MenuItemId = menuItemId,
            InventoryItemId = ingredientId,
            QuantityRequired = 0.5m,
            InventoryItem = inventoryItem
        };

        await context.Orders.AddAsync(order);
        await context.Payments.AddAsync(payment);
        await context.InventoryItems.AddAsync(inventoryItem);
        await context.MenuItemIngredients.AddAsync(menuItemIngredient);
        await context.SaveChangesAsync();

        var stripeEvent = new
        {
            id = "evt_test",
            @object = "event",
            api_version = "2020-08-27",
            created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            data = new
            {
                @object = new
                {
                    id = "pi_test",
                    @object = "payment_intent",
                    metadata = new { orderId = orderId.ToString() }
                }
            },
            livemode = false,
            pending_webhooks = 1,
            request = new { id = "req_test", idempotency_key = (string?)null },
            type = "payment_intent.succeeded"
        };

        var payload = JsonSerializer.Serialize(stripeEvent);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var signatureHeader = CreateSignatureHeader(payload, secret, timestamp);

        var result = await handler.Handle(new StripeWebhookRequest
        {
            Payload = payload,
            Signature = signatureHeader
        }, CancellationToken.None);

        result.Should().BeAssignableTo<IStatusCodeHttpResult>();
        ((IStatusCodeHttpResult)result).StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var updatedPayment = await context.Payments.FirstAsync();
        updatedPayment.Status.Should().Be("pending");
        updatedPayment.TransactionId.Should().BeNull();

        var updatedOrder = await context.Orders.Include(o => o.Items).FirstAsync();
        updatedOrder.Status.Should().Be("Pending");

        var updatedInventory = await context.InventoryItems.FirstAsync();
        updatedInventory.CurrentQuantity.Should().Be(10m);
    }

    [Fact]
    public async Task GivenFailedPaymentIntent_WhenSignatureValid_ThenMarksPaymentFailed()
    {
        await using var context = CreateContext();
        var secret = "whsec_test";
        var config = CreateConfig(secret);
        var handler = new StripeWebhookHandler(context, config);

        var orderId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Amount = 22m,
            Status = "pending",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await context.Payments.AddAsync(payment);
        await context.SaveChangesAsync();

        var stripeEvent = new
        {
            id = "evt_test",
            @object = "event",
            api_version = "2020-08-27",
            created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            data = new
            {
                @object = new
                {
                    id = "pi_test_failed",
                    @object = "payment_intent",
                    metadata = new { orderId = orderId.ToString() }
                }
            },
            livemode = false,
            pending_webhooks = 1,
            request = new { id = "req_test", idempotency_key = (string?)null },
            type = "payment_intent.payment_failed"
        };

        var payload = JsonSerializer.Serialize(stripeEvent);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var signatureHeader = CreateSignatureHeader(payload, secret, timestamp);

        var result = await handler.Handle(new StripeWebhookRequest
        {
            Payload = payload,
            Signature = signatureHeader
        }, CancellationToken.None);

        result.Should().BeAssignableTo<IStatusCodeHttpResult>();
        ((IStatusCodeHttpResult)result).StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var updatedPayment = await context.Payments.FirstAsync();
        updatedPayment.Status.Should().Be("pending");
        updatedPayment.TransactionId.Should().BeNull();
    }
}
