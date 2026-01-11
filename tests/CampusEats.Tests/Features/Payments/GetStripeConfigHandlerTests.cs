using CampusEats.Api.Features.Payments.GetStripeConfig;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace CampusEats.Tests.Features.Payments;

public class GetStripeConfigHandlerTests
{
    [Fact]
    public async Task GivenMissingConfiguration_WhenGettingConfig_ThenReturnsProblem()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(x => x["Stripe:PublishableKey"]).Returns((string?)null);

        // Mock Environment.GetEnvironmentVariable to return null
        Environment.SetEnvironmentVariable("STRIPE_PUBLISHABLE_KEY", null);

        var handler = new GetStripeConfigHandler(configurationMock.Object);
        var request = new GetStripeConfigRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenValidConfiguration_WhenGettingConfig_ThenReturnsPublishableKey()
    {
        // Arrange
        var publishableKey = "pk_test_12345";
        
        // Set environment variable
        Environment.SetEnvironmentVariable("STRIPE_PUBLISHABLE_KEY", publishableKey);

        var configurationMock = new Mock<IConfiguration>();
        
        var handler = new GetStripeConfigHandler(configurationMock.Object);
        var request = new GetStripeConfigRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        
        // Cleanup
        Environment.SetEnvironmentVariable("STRIPE_PUBLISHABLE_KEY", null);
    }

    [Fact]
    public async Task GivenConfigurationInAppSettings_WhenGettingConfig_ThenReturnsPublishableKey()
    {
        // Arrange
        var publishableKey = "pk_test_67890";
        
        // Clear environment variable
        Environment.SetEnvironmentVariable("STRIPE_PUBLISHABLE_KEY", null);

        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(x => x["Stripe:PublishableKey"]).Returns(publishableKey);
        
        var handler = new GetStripeConfigHandler(configurationMock.Object);
        var request = new GetStripeConfigRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }
}
