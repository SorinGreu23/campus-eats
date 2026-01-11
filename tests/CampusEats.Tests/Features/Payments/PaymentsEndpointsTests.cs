using System.Linq;
using CampusEats.Api.Features.Payments;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CampusEats.Tests.Features.Payments;

public class PaymentsEndpointsTests
{
    private static WebApplication CreateApp()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
        var app = builder.Build();
        app.MapPaymentsEndpoints();
        return app;
    }

    private static RouteEndpoint? FindByPattern(WebApplication app, string pattern, string method)
    {
        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(ds => ds.Endpoints).OfType<RouteEndpoint>();
        return endpoints.FirstOrDefault(e => e.RoutePattern.RawText == pattern && e.Metadata.OfType<HttpMethodMetadata>().Any(m => m.HttpMethods.Contains(method)));
    }

    private static RouteEndpoint? FindByName(WebApplication app, string name)
    {
        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(ds => ds.Endpoints).OfType<RouteEndpoint>();
        return endpoints.FirstOrDefault(e => e.Metadata.OfType<EndpointNameMetadata>().Any(n => n.EndpointName == name));
    }

    [Fact]
    public void MapsGetStripeConfig_WithAuthorization()
    {
        var app = CreateApp();
        var ep = FindByPattern(app, "/api/payments/config", "GET");
        ep.Should().NotBeNull();
        FindByName(app, "GetStripeConfig").Should().NotBeNull();
        ep!.Metadata.OfType<IAuthorizeData>().Should().NotBeEmpty();
    }

    [Fact]
    public void MapsCreatePaymentIntent_WithAuthorization()
    {
        var app = CreateApp();
        var ep = FindByPattern(app, "/api/payments/create-payment-intent", "POST");
        ep.Should().NotBeNull();
        FindByName(app, "CreatePaymentIntent").Should().NotBeNull();
        ep!.Metadata.OfType<IAuthorizeData>().Should().NotBeEmpty();
    }

    [Fact]
    public void MapsConfirmPayment_WithAuthorization()
    {
        var app = CreateApp();
        var ep = FindByPattern(app, "/api/payments/confirm", "POST");
        ep.Should().NotBeNull();
        FindByName(app, "ConfirmPayment").Should().NotBeNull();
        ep!.Metadata.OfType<IAuthorizeData>().Should().NotBeEmpty();
    }

    [Fact]
    public void MapsTestConfirmPayment_WithAuthorization()
    {
        var app = CreateApp();
        var ep = FindByPattern(app, "/api/payments/test-confirm", "POST");
        ep.Should().NotBeNull();
        FindByName(app, "TestConfirmPayment").Should().NotBeNull();
        ep!.Metadata.OfType<IAuthorizeData>().Should().NotBeEmpty();
    }

    [Fact]
    public void MapsStripeWebhook_AllowAnonymous()
    {
        var app = CreateApp();
        var ep = FindByPattern(app, "/api/payments/webhook", "POST");
        ep.Should().NotBeNull();
        FindByName(app, "StripeWebhook").Should().NotBeNull();
        ep!.Metadata.OfType<IAllowAnonymous>().Should().NotBeEmpty();
    }
}
