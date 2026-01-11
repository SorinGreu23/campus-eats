using System.Linq;
using CampusEats.Api.Features.Menu;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Authorization;

namespace CampusEats.Tests.Features.Menu;

public class MenuEndpointsTests
{
    private static WebApplication CreateApp()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
        var app = builder.Build();
        app.MapMenuEndpoints();
        return app;
    }

    private static RouteEndpoint? FindEndpointByPattern(WebApplication app, string pattern, string httpMethod)
    {
        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(ds => ds.Endpoints).OfType<RouteEndpoint>();
        return endpoints.FirstOrDefault(e => e.RoutePattern.RawText == pattern && e.Metadata.OfType<HttpMethodMetadata>().Any(m => m.HttpMethods.Contains(httpMethod)));
    }

    private static RouteEndpoint? FindEndpointByName(WebApplication app, string name)
    {
        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(ds => ds.Endpoints).OfType<RouteEndpoint>();
        return endpoints.FirstOrDefault(e => e.Metadata.OfType<EndpointNameMetadata>().Any(n => n.EndpointName == name));
    }

    [Fact]
    public void MapsCategoriesGetEndpoint()
    {
        var app = CreateApp();
        var ep = FindEndpointByPattern(app, "/api/categories", "GET");
        ep.Should().NotBeNull();
        ep!.Metadata.OfType<EndpointNameMetadata>().FirstOrDefault()?.EndpointName.Should().Be("GetCategories");
    }

    [Fact]
    public void MapsMenuItemsListEndpoint()
    {
        var app = CreateApp();
        var ep = FindEndpointByPattern(app, "/api/menuitems/", "GET");
        ep.Should().NotBeNull();
        FindEndpointByName(app, "GetMenuItems").Should().NotBeNull();
    }

    [Fact]
    public void MapsMenuItemByIdEndpoint()
    {
        var app = CreateApp();
        var ep = FindEndpointByPattern(app, "/api/menuitems/{id:guid}", "GET");
        ep.Should().NotBeNull();
        FindEndpointByName(app, "GetMenuItemById").Should().NotBeNull();
    }

    [Fact]
    public void CreateMenuItemEndpoint_RequiresAdminOrKitchen()
    {
        var app = CreateApp();
        var ep = FindEndpointByName(app, "CreateMenuItem");
        ep.Should().NotBeNull();
        ep!.Metadata.OfType<HttpMethodMetadata>().Any(m => m.HttpMethods.Contains("POST")).Should().BeTrue();
        var authData = ep.Metadata.OfType<IAuthorizeData>().ToList();
        authData.Should().NotBeEmpty();
    }

    [Fact]
    public void AddIngredientEndpoint_RequiresAdminOrKitchen()
    {
        var app = CreateApp();
        var ep = FindEndpointByName(app, "AddMenuItemIngredient");
        ep.Should().NotBeNull();
        ep!.Metadata.OfType<HttpMethodMetadata>().Any(m => m.HttpMethods.Contains("POST")).Should().BeTrue();
        var authData = ep.Metadata.OfType<IAuthorizeData>().ToList();
        authData.Should().NotBeEmpty();
    }

    [Fact]
    public void UpdateIngredientEndpoint_RequiresAdminOrKitchen()
    {
        var app = CreateApp();
        var ep = FindEndpointByName(app, "UpdateMenuItemIngredient");
        ep.Should().NotBeNull();
        ep!.Metadata.OfType<HttpMethodMetadata>().Any(m => m.HttpMethods.Contains("PUT")).Should().BeTrue();
        var authData = ep.Metadata.OfType<IAuthorizeData>().ToList();
        authData.Should().NotBeEmpty();
    }

    [Fact]
    public void DeleteIngredientEndpoint_RequiresAdminOrKitchen()
    {
        var app = CreateApp();
        var ep = FindEndpointByName(app, "DeleteMenuItemIngredient");
        ep.Should().NotBeNull();
        ep!.Metadata.OfType<HttpMethodMetadata>().Any(m => m.HttpMethods.Contains("DELETE")).Should().BeTrue();
        var authData = ep.Metadata.OfType<IAuthorizeData>().ToList();
        authData.Should().NotBeEmpty();
    }
}
