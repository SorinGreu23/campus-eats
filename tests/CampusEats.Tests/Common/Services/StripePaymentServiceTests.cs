using CampusEats.Api.Common.Interfaces;
using CampusEats.Api.Common.Services;
using FluentAssertions;
using Moq;
using Stripe;

namespace CampusEats.Tests.Common.Services;

public class StripePaymentServiceTests
{
    private readonly Mock<IPaymentIntentService> _paymentIntentServiceMock;
    private readonly StripePaymentService _sut;

    public StripePaymentServiceTests()
    {
        _paymentIntentServiceMock = new Mock<IPaymentIntentService>();
        _sut = new StripePaymentService(_paymentIntentServiceMock.Object);
    }

    [Fact]
    public void GivenMissingSecretKey_WhenConstructing_ThenThrowsInvalidOperationException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("STRIPE_SECRET_KEY", null);

        // Act
        Action act = () => new StripePaymentService();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Stripe*");
    }

    [Fact]
    public async Task GivenValidAmount_WhenCreatingPaymentIntent_ThenConvertsToCents()
    {
        // Arrange
        var expectedClientSecret = "pi_test_secret";
        var paymentIntent = new PaymentIntent { ClientSecret = expectedClientSecret };
        
        _paymentIntentServiceMock
            .Setup(x => x.CreateAsync(It.Is<PaymentIntentCreateOptions>(o => o.Amount == 1050), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentIntent);

        // Act
        var result = await _sut.CreatePaymentIntentAsync(10.50m, "usd", "user123", Guid.NewGuid());

        // Assert
        result.Should().Be(expectedClientSecret);
        _paymentIntentServiceMock.Verify(
            x => x.CreateAsync(It.Is<PaymentIntentCreateOptions>(o => o.Amount == 1050 && o.Currency == "usd"), It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task GivenSucceededStatus_WhenConfirmingPayment_ThenReturnsTrue()
    {
        // Arrange
        var paymentIntentId = "pi_test_123";
        var paymentIntent = new PaymentIntent { Status = "succeeded" };
        
        _paymentIntentServiceMock
            .Setup(x => x.GetAsync(paymentIntentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentIntent);

        // Act
        var result = await _sut.ConfirmPaymentAsync(paymentIntentId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GivenOtherStatus_WhenConfirmingPayment_ThenReturnsFalse()
    {
        // Arrange
        var paymentIntentId = "pi_test_123";
        var paymentIntent = new PaymentIntent { Status = "requires_payment_method" };
        
        _paymentIntentServiceMock
            .Setup(x => x.GetAsync(paymentIntentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentIntent);

        // Act
        var result = await _sut.ConfirmPaymentAsync(paymentIntentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GivenValidPaymentIntent_WhenCancelling_ThenReturnsTrueIfCanceled()
    {
        // Arrange
        var paymentIntentId = "pi_test_123";
        var paymentIntent = new PaymentIntent { Status = "canceled" };
        
        _paymentIntentServiceMock
            .Setup(x => x.CancelAsync(paymentIntentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentIntent);

        // Act
        var result = await _sut.CancelPaymentAsync(paymentIntentId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GivenException_WhenCancelling_ThenReturnsFalse()
    {
        // Arrange
        var paymentIntentId = "pi_test_123";
        
        _paymentIntentServiceMock
            .Setup(x => x.CancelAsync(paymentIntentId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new StripeException("Payment intent cannot be canceled"));

        // Act
        var result = await _sut.CancelPaymentAsync(paymentIntentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GivenPaymentIntentId_WhenGettingStatus_ThenReturnsStatus()
    {
        // Arrange
        var paymentIntentId = "pi_test_123";
        var expectedStatus = "processing";
        var paymentIntent = new PaymentIntent { Status = expectedStatus };
        
        _paymentIntentServiceMock
            .Setup(x => x.GetAsync(paymentIntentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentIntent);

        // Act
        var result = await _sut.GetPaymentStatusAsync(paymentIntentId);

        // Assert
        result.Should().Be(expectedStatus);
    }
}
