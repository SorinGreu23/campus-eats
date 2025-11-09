using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CampusEats.Api.Features.Menu;

public static class MenuEndpoints
{
    public static IEndpointRouteBuilder MapMenuEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/menuitems")
            .WithTags("MenuItems");

        group.MapGet("/", async ([FromServices] IMediator mediator) =>
        {
            var result = await mediator.Send(new GetMenuItems.Query());
            return Results.Ok(result);
        })
        .WithName("GetMenuItems")
        .Produces<List<GetMenuItems.Response>>();

        group.MapGet("/{id:guid}", async (Guid id, [FromServices] IMediator mediator) =>
        {
            var result = await mediator.Send(new GetMenuItemById.Query(id));
            return result is not null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("GetMenuItemById")
        .Produces<GetMenuItemById.Response>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async ([FromBody] CreateItemHandler.Command command, [FromServices] IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Created($"/api/menuitems/{result.Id}", result);
        })
        .WithName("CreateMenuItem")
        .Produces<CreateItemHandler.Response>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateMenuItem.Command command,
                [FromServices] IMediator mediator) =>
        {
            if (id != command.Id)
                return Results.BadRequest("Id in URL does not match Id in body");

            var result = await mediator.Send(command);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateMenuItem")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:guid}", async (Guid id, [FromServices] IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteMenuItem.Command(id));
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteMenuItem")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}

