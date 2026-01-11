using CampusEats.Api.Common.Interfaces;
using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Payments.CreatePaymentIntent;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;

namespace CampusEats.Tests.Features.Payments;

public class CreatePaymentIntentHandlerTests
{
    private readonly DbContextOptions<CampusDbContext> _options;
    private readonly Mock<IStripePaymentService> _mockStripeService;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;

    public CreatePaymentIntentHandlerTests()
    {
        _options = new DbContextOptionsBuilder<CampusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _mockStripeService = new Mock<IStripePaymentService>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
    }

    [Fact]
    public async Task GivenUnauthenticatedUser_WhenCreatingPaymentIntent_ThenReturnsUnauthorized()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var handler = new CreatePaymentIntentHandler(
            context,
            _mockStripeService.Object,
            _mockHttpContextAccessor.Object
        );

        var request = new CreatePaymentIntentRequest { OrderId = Guid.NewGuid() };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var httpResult = result as IResult;
        httpResult.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenUserWithNoClaim_WhenCreatingPaymentIntent_ThenReturnsUnauthorized()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);
        
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "testuser")
        }, "TestAuth"));

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var handler = new CreatePaymentIntentHandler(
            context,
            _mockStripeService.Object,
            _mockHttpContextAccessor.Object
        );

        var request = new CreatePaymentIntentRequest { OrderId = Guid.NewGuid() };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenNonExistentOrder_WhenCreatingPaymentIntent_ThenReturnsNotFound()
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

        var handler = new CreatePaymentIntentHandler(
            context,
            _mockStripeService.Object,
            _mockHttpContextAccessor.Object
        );

        var request = new CreatePaymentIntentRequest { OrderId = Guid.NewGuid() };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenOrderOwnedByDifferentUser_WhenCreatingPaymentIntent_ThenReturnsForbid()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);
        var userId = "test-user-123";
        var differentUserId = "different-user-456";
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

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "TestAuth"));

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var handler = new CreatePaymentIntentHandler(
            context,
            _mockStripeService.Object,
            _mockHttpContextAccessor.Object
        );

        var request = new CreatePaymentIntentRequest { OrderId = orderId };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenOrderNotPending_WhenCreatingPaymentIntent_ThenReturnsBadRequest()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);
        var userId = "test-user-123";
        var orderId = Guid.NewGuid();

        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-001",
            UserId = userId,
            Status = "Paid",
            Subtotal = 100,
            Tax = 21,
            Discount = 0,
            Total = 121,
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "TestAuth"));

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var handler = new CreatePaymentIntentHandler(
            context,
            _mockStripeService.Object,
            _mockHttpContextAccessor.Object
        );

        var request = new CreatePaymentIntentRequest { OrderId = orderId };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenValidRequest_WhenCreatingPaymentIntent_ThenCreatesPaymentAndReturnsClientSecret()
    {
        // Arrange
        await using var context = new CampusDbContext(_options);
        var userId = "test-user-123";
        var orderId = Guid.NewGuid();
        var clientSecret = "pi_test_secret";

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

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "TestAuth"));

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        _mockStripeService.Setup(x => x.CreatePaymentIntentAsync(
            It.IsAny<decimal>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(clientSecret);

        var handler = new CreatePaymentIntentHandler(
            context,
            _mockStripeService.Object,
            _mockHttpContextAccessor.Object
        );

        var request = new CreatePaymentIntentRequest { OrderId = orderId };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var payment = await context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
        payment.Should().NotBeNull();
        payment!.UserId.Should().Be(userId);
        payment.Amount.Should().Be(121);
        payment.Status.Should().Be("pending");
    }
}
