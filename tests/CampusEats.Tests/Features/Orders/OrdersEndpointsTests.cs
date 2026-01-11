using System.Linq;
using System.Security.Claims;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Orders;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace CampusEats.Tests.Features.Orders;

public class OrdersEndpointsTests
{
    private static WebApplication CreateApp()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
        
        // Register required services
        builder.Services.AddSingleton<IMediator>(new NoopMediator());
        builder.Services.AddSingleton<UserManager<ApplicationUser>>(sp => null!); // Mock will be set up per test
        
        var app = builder.Build();
        app.MapOrdersEndpoints();
        return app;
    }

    private static RouteEndpoint? FindEndpointByPattern(WebApplication app, string pattern, string httpMethod)
    {
        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(ds => ds.Endpoints).OfType<RouteEndpoint>();
        return endpoints.FirstOrDefault(e => 
            e.RoutePattern.RawText == pattern && 
            e.Metadata.OfType<HttpMethodMetadata>().Any(m => m.HttpMethods.Contains(httpMethod)));
    }

    private static RouteEndpoint? FindEndpointByName(WebApplication app, string name)
    {
        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(ds => ds.Endpoints).OfType<RouteEndpoint>();
        return endpoints.FirstOrDefault(e => 
            e.Metadata.OfType<EndpointNameMetadata>().Any(n => n.EndpointName == name));
    }

    [Fact]
    public void MapsGetPendingOrdersEndpoint()
    {
        var app = CreateApp();
        var ep = FindEndpointByPattern(app, "/api/orders/pending", "GET");
        
        ep.Should().NotBeNull();
        ep!.Metadata.OfType<EndpointNameMetadata>().FirstOrDefault()?.EndpointName.Should().Be("GetPendingOrders");
    }

    [Fact]
    public void GetPendingOrdersEndpoint_AllowsAnonymous()
    {
        var app = CreateApp();
        var ep = FindEndpointByName(app, "GetPendingOrders");
        
        ep.Should().NotBeNull();
        ep!.Metadata.OfType<IAllowAnonymous>().Should().NotBeEmpty();
    }

    [Fact]
    public void MapsCreateOrderEndpoint()
    {
        var app = CreateApp();
        var ep = FindEndpointByPattern(app, "/api/orders/", "POST");
        
        ep.Should().NotBeNull();
        ep!.Metadata.OfType<EndpointNameMetadata>().FirstOrDefault()?.EndpointName.Should().Be("CreateOrder");
    }

    [Fact]
    public void CreateOrderEndpoint_RequiresAuthorization()
    {
        var app = CreateApp();
        var ep = FindEndpointByName(app, "CreateOrder");
        
        ep.Should().NotBeNull();
        ep!.Metadata.OfType<IAuthorizeData>().Should().NotBeEmpty();
    }

    [Fact]
    public void MapsGetOrdersByUserEndpoint()
    {
        var app = CreateApp();
        var ep = FindEndpointByPattern(app, "/api/orders/user/me", "GET");
        
        ep.Should().NotBeNull();
        ep!.Metadata.OfType<EndpointNameMetadata>().FirstOrDefault()?.EndpointName.Should().Be("GetOrdersByUser");
    }

    [Fact]
    public void GetOrdersByUserEndpoint_RequiresAuthorization()
    {
        var app = CreateApp();
        var ep = FindEndpointByName(app, "GetOrdersByUser");
        
        ep.Should().NotBeNull();
        ep!.Metadata.OfType<IAuthorizeData>().Should().NotBeEmpty();
    }

    [Fact]
    public void MapsCancelOrderEndpoint()
    {
        var app = CreateApp();
        var ep = FindEndpointByPattern(app, "/api/orders/{orderId:guid}/cancel", "PATCH");
        
        ep.Should().NotBeNull();
        ep!.Metadata.OfType<EndpointNameMetadata>().FirstOrDefault()?.EndpointName.Should().Be("CancelOrder");
    }

    [Fact]
    public void CancelOrderEndpoint_RequiresAuthorization()
    {
        var app = CreateApp();
        var ep = FindEndpointByName(app, "CancelOrder");
        
        ep.Should().NotBeNull();
        ep!.Metadata.OfType<IAuthorizeData>().Should().NotBeEmpty();
    }

    [Fact]
    public void MapsCompleteOrderEndpoint()
    {
        var app = CreateApp();
        var ep = FindEndpointByPattern(app, "/api/orders/{orderId:guid}/complete", "PATCH");
        
        ep.Should().NotBeNull();
        ep!.Metadata.OfType<EndpointNameMetadata>().FirstOrDefault()?.EndpointName.Should().Be("CompleteOrder");
    }

    [Fact]
    public void CompleteOrderEndpoint_RequiresAuthorization()
    {
        var app = CreateApp();
        var ep = FindEndpointByName(app, "CompleteOrder");
        
        ep.Should().NotBeNull();
        ep!.Metadata.OfType<IAuthorizeData>().Should().NotBeEmpty();
    }

    [Fact]
    public void MapsUpdateOrderStatusEndpoint()
    {
        var app = CreateApp();
        var ep = FindEndpointByPattern(app, "/api/orders/status", "PATCH");
        
        ep.Should().NotBeNull();
    }

    [Fact]
    public void UpdateOrderStatusEndpoint_RequiresAuthorization()
    {
        var app = CreateApp();
        var ep = FindEndpointByPattern(app, "/api/orders/status", "PATCH");
        
        ep.Should().NotBeNull();
        ep!.Metadata.OfType<IAuthorizeData>().Should().NotBeEmpty();
    }

    private class NoopMediator : IMediator
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) 
            => Task.FromResult(default(TResponse)!);
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest 
            => Task.CompletedTask;
        public Task<object?> Send(object request, CancellationToken cancellationToken = default) 
            => Task.FromResult<object?>(null);
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) 
            => EmptyAsync<TResponse>();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) 
            => EmptyAsync<object?>();
        public Task Publish(object notification, CancellationToken cancellationToken = default) 
            => Task.CompletedTask;
        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification 
            => Task.CompletedTask;

        private static async IAsyncEnumerable<T> EmptyAsync<T>()
        {
            yield break;
        }
    }
}
