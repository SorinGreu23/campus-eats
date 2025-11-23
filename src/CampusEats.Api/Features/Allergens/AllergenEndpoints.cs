﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using CampusEats.Api.Features.Menu;

namespace CampusEats.Api.Features.Allergens;
public static class AllergenEndpoints
{
    public static IEndpointRouteBuilder MapAllergenEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/allergens")
            .WithTags("Allergens");
        group.MapGet("/", async ([FromServices] IMediator mediator) =>
        {
            return await mediator.Send(new GetAllergensRequest());
        })
        .WithName("GetAllergens")
        .Produces<List<AllergenDto>>();
        return app;
    }
}
