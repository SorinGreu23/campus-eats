using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CampusEats.Api.Features.Users;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace CampusEats.Tests.Features.Users;

public class UserEndpointsTests
{
    private static WebApplication CreateApp()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
        builder.Services.AddSingleton<IMediator, NoopMediator>();
        var app = builder.Build();
        app.MapUserEndpoints();
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
    public void MapsRegisterUser_AllowsAnonymous()
    {
        var app = CreateApp();
        var ep = FindByPattern(app, "/api/users/register", "POST");
        ep.Should().NotBeNull();
        FindByName(app, "RegisterUser").Should().NotBeNull();
        ep!.Metadata.OfType<IAuthorizeData>().Should().BeEmpty();
    }

    [Fact]
    public void MapsLoginUser_AllowsAnonymous()
    {
        var app = CreateApp();
        var ep = FindByPattern(app, "/api/users/login", "POST");
        ep.Should().NotBeNull();
        FindByName(app, "LoginUser").Should().NotBeNull();
        ep!.Metadata.OfType<IAuthorizeData>().Should().BeEmpty();
    }

    [Fact]
    public void MapsGetUsers_RequiresAdminAuthorization()
    {
        var app = CreateApp();
        var ep = FindByPattern(app, "/api/users", "GET");
        ep.Should().NotBeNull();
        FindByName(app, "GetUsers").Should().NotBeNull();
        ep!.Metadata.OfType<IAuthorizeData>().Should().NotBeEmpty();
    }

    [Fact]
    public void MapsGetUser_AllowsAnonymous()
    {
        var app = CreateApp();
        var ep = FindByPattern(app, "/api/users/{id}", "GET");
        ep.Should().NotBeNull();
        FindByName(app, "GetUser").Should().NotBeNull();
        ep!.Metadata.OfType<IAuthorizeData>().Should().BeEmpty();
    }

    [Fact]
    public void MapsUpdateUser_RequiresAuthorization()
    {
        var app = CreateApp();
        var ep = FindByPattern(app, "/api/users/{id}", "PUT");
        ep.Should().NotBeNull();
        FindByName(app, "UpdateUser").Should().NotBeNull();
        ep!.Metadata.OfType<IAuthorizeData>().Should().NotBeEmpty();
    }

    [Fact]
    public void MapsDeleteUser_RequiresAuthorization()
    {
        var app = CreateApp();
        var ep = FindByPattern(app, "/api/users/{id}", "DELETE");
        ep.Should().NotBeNull();
        FindByName(app, "DeleteUser").Should().NotBeNull();
        ep!.Metadata.OfType<IAuthorizeData>().Should().NotBeEmpty();
    }

    private sealed class NoopMediator : IMediator
    {
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => Task.CompletedTask;

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) => Task.FromResult(default(TResponse)!);

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => Task.FromResult<object?>(null);

        public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification => Task.CompletedTask;

        public async IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield break;
        }

        public async IAsyncEnumerable<object?> CreateStream(object request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield break;
        }
    }
}
