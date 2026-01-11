using System.Linq;
using CampusEats.Api.Features.Inventory;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CampusEats.Tests.Features.Inventory;

public class InventoryEndpointsTests
{
    private WebApplication BuildApp()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Development"
        });
        builder.Services.AddSingleton<IMediator>(new NoopMediator());
        var app = builder.Build();
        InventoryEndpoints.MapInventoryEndpoints(app);
        return app;
    }

    private class NoopMediator : IMediator
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) => Task.FromResult(default(TResponse)!);
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => Task.CompletedTask;
        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => Task.FromResult<object?>(null);
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => EmptyAsync<TResponse>();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => EmptyAsync<object?>();
        public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification => Task.CompletedTask;
    }

    private static async IAsyncEnumerable<T> EmptyAsync<T>()
    {
        yield break;
    }

    [Fact]
    public void Maps_GetAllInventoryItems()
    {
        var app = BuildApp();
        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(ds => ds.Endpoints).ToList();

        var match = endpoints.FirstOrDefault(e => (e as RouteEndpoint)?.RoutePattern.RawText == "/api/inventory/");
        match.Should().NotBeNull();

        var httpMethods = match!.Metadata.OfType<HttpMethodMetadata>().Single();
        httpMethods.HttpMethods.Should().ContainSingle().Which.Should().Be("GET");
    }

    [Fact]
    public void Maps_GetInventoryItemById()
    {
        var app = BuildApp();
        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(ds => ds.Endpoints).ToList();

        var match = endpoints.FirstOrDefault(e => (e as RouteEndpoint)?.RoutePattern.RawText == "/api/inventory/{id:guid}");
        match.Should().NotBeNull();

        var httpMethods = match!.Metadata.OfType<HttpMethodMetadata>().Single();
        httpMethods.HttpMethods.Should().ContainSingle().Which.Should().Be("GET");
    }

    [Fact]
    public void Maps_RestockInventoryItem()
    {
        var app = BuildApp();
        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(ds => ds.Endpoints).ToList();

        var match = endpoints.FirstOrDefault(e => (e as RouteEndpoint)?.RoutePattern.RawText == "/api/inventory/{id:guid}/restock");
        match.Should().NotBeNull();

        var httpMethods = match!.Metadata.OfType<HttpMethodMetadata>().Single();
        httpMethods.HttpMethods.Should().ContainSingle().Which.Should().Be("POST");
    }

    [Fact]
    public void Maps_UseInventoryItem()
    {
        var app = BuildApp();
        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(ds => ds.Endpoints).ToList();

        var match = endpoints.FirstOrDefault(e => (e as RouteEndpoint)?.RoutePattern.RawText == "/api/inventory/{id:guid}/use");
        match.Should().NotBeNull();

        var httpMethods = match!.Metadata.OfType<HttpMethodMetadata>().Single();
        httpMethods.HttpMethods.Should().ContainSingle().Which.Should().Be("POST");
    }
}
