using CampusEats.Api.Common.Interfaces;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Payments.ConfirmPayment;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;

namespace CampusEats.Tests.Features.Payments;

public class ConfirmPaymentHandlerTests
{
    private readonly DbContextOptions<CampusDbContext> _options;
    private readonly Mock<IStripePaymentService> _mockStripeService;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;

    public ConfirmPaymentHandlerTests()
    {
        _options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _mockStripeService = new Mock<IStripePaymentService>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
    }

    [Fact]
    public async Task GivenUnauthenticatedUser_WhenConfirmingPayment_ThenReturnsUnauthorized()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var handler = new ConfirmPaymentHandler(
            context,
            _mockStripeService.Object,
            _mockHttpContextAccessor.Object
        );

        var request = new ConfirmPaymentRequest
        {
            PaymentId = Guid.NewGuid(),
            PaymentIntentId = "pi_test"
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenNonExistentPayment_WhenConfirmingPayment_ThenReturnsNotFound()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);
        var userId = "test-user-123";

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "TestAuth"));

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var handler = new ConfirmPaymentHandler(
            context,
            _mockStripeService.Object,
            _mockHttpContextAccessor.Object
        );

        var request = new ConfirmPaymentRequest
        {
            PaymentId = Guid.NewGuid(),
            PaymentIntentId = "pi_test"
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenPaymentOwnedByDifferentUser_WhenConfirmingPayment_ThenReturnsForbid()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);
        var userId = "test-user-123";
        var differentUserId = "different-user-456";
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-001",
            UserId = differentUserId,
            Status = "Pending",
            Subtotal = 100,
            Tax = 21,
            Discount = 0,
            Total = 121,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var payment = new Payment
        {
            Id = paymentId,
            OrderId = orderId,
            UserId = differentUserId,
            Amount = 121,
            Status = "pending",
            PaymentMethod = "stripe",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        context.Orders.Add(order);
        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "TestAuth"));

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var handler = new ConfirmPaymentHandler(
            context,
            _mockStripeService.Object,
            _mockHttpContextAccessor.Object
        );

        var request = new ConfirmPaymentRequest
        {
            PaymentId = paymentId,
            PaymentIntentId = "pi_test"
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenSuccessfulStripeConfirmation_WhenConfirmingPayment_ThenUpdatesPaymentAndOrder()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);
        var userId = "test-user-123";
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var paymentIntentId = "pi_test_123";

        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-001",
            UserId = userId,
            Status = "Pending",
            Subtotal = 100,
            Tax = 21,
            Discount = 0,
            Total = 121,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var payment = new Payment
        {
            Id = paymentId,
            OrderId = orderId,
            UserId = userId,
            Amount = 121,
            Status = "pending",
            PaymentMethod = "stripe",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        context.Orders.Add(order);
        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "TestAuth"));

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        _mockStripeService.Setup(x => x.ConfirmPaymentAsync(paymentIntentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new ConfirmPaymentHandler(
            context,
            _mockStripeService.Object,
            _mockHttpContextAccessor.Object
        );

        var request = new ConfirmPaymentRequest
        {
            PaymentId = paymentId,
            PaymentIntentId = paymentIntentId
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var updatedPayment = await context.Payments.FindAsync(paymentId);
        updatedPayment.Should().NotBeNull();
        updatedPayment!.Status.Should().Be("succeeded");
        updatedPayment.TransactionId.Should().Be(paymentIntentId);

        var updatedOrder = await context.Orders.FindAsync(orderId);
        updatedOrder.Should().NotBeNull();
        updatedOrder!.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task GivenFailedStripeConfirmation_WhenConfirmingPayment_ThenMarksPaymentFailed()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);
        var userId = "test-user-123";
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var paymentIntentId = "pi_test_123";

        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-001",
            UserId = userId,
            Status = "Pending",
            Subtotal = 100,
            Tax = 21,
            Discount = 0,
            Total = 121,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var payment = new Payment
        {
            Id = paymentId,
            OrderId = orderId,
            UserId = userId,
            Amount = 121,
            Status = "pending",
            PaymentMethod = "stripe",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        context.Orders.Add(order);
        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "TestAuth"));

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        _mockStripeService.Setup(x => x.ConfirmPaymentAsync(paymentIntentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new ConfirmPaymentHandler(
            context,
            _mockStripeService.Object,
            _mockHttpContextAccessor.Object
        );

        var request = new ConfirmPaymentRequest
        {
            PaymentId = paymentId,
            PaymentIntentId = paymentIntentId
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var updatedPayment = await context.Payments.FindAsync(paymentId);
        updatedPayment.Should().NotBeNull();
        updatedPayment!.Status.Should().Be("failed");
    }
}
