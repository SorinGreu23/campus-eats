using System.Linq;
using CampusEats.Api.Features.Allergens;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace CampusEats.Tests.Features.Allergens;

public class AllergenEndpointsTests
{
    private static WebApplication CreateApp()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
        var app = builder.Build();
        app.MapAllergenEndpoints();
        return app;
    }

    [Fact]
    public void MapsGetAllergensEndpoint()
    {
        var app = CreateApp();
        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(ds => ds.Endpoints).OfType<RouteEndpoint>();
        var ep = endpoints.FirstOrDefault(e => e.RoutePattern.RawText == "/api/allergens/" && e.Metadata.OfType<HttpMethodMetadata>().Any(m => m.HttpMethods.Contains("GET")));
        ep.Should().NotBeNull();
        ep!.Metadata.OfType<EndpointNameMetadata>().FirstOrDefault()?.EndpointName.Should().Be("GetAllergens");
    }
}
