﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using CampusEats.Api.Features.Menu;

namespace CampusEats.Api.Features.DietaryRestrictions;
public static class DietaryRestrictionEndpoints
{
    public static IEndpointRouteBuilder MapDietaryRestrictionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dietary-restrictions")
            .WithTags("DietaryRestrictions");
        group.MapGet("/", async ([FromServices] IMediator mediator) =>
        {
            return await mediator.Send(new GetDietaryRestrictionsRequest());
        })
        .WithName("GetDietaryRestrictions")
        .Produces<List<DietaryRestrictionDto>>();
        return app;
    }
}
