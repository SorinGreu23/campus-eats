using System.Linq;
using CampusEats.Api.Features.DietaryRestrictions;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace CampusEats.Tests.Features.DietaryRestrictions;

public class DietaryRestrictionEndpointsTests
{
    private static WebApplication CreateApp()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
        var app = builder.Build();
        app.MapDietaryRestrictionEndpoints();
        return app;
    }

    [Fact]
    public void MapsGetDietaryRestrictionsEndpoint()
    {
        var app = CreateApp();
        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(ds => ds.Endpoints).OfType<RouteEndpoint>();
        var ep = endpoints.FirstOrDefault(e => e.RoutePattern.RawText == "/api/dietary-restrictions/" && e.Metadata.OfType<HttpMethodMetadata>().Any(m => m.HttpMethods.Contains("GET")));
        ep.Should().NotBeNull();
        ep!.Metadata.OfType<EndpointNameMetadata>().FirstOrDefault()?.EndpointName.Should().Be("GetDietaryRestrictions");
    }
}
