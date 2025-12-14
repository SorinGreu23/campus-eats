using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CampusEats.Api.Features.Menu;

public static class MenuEndpoints
{
    public static IEndpointRouteBuilder MapMenuEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/menuitems").WithTags("MenuItems");

        group.MapGet("/", async (
            [FromQuery] Guid? categoryId,
            [FromQuery] Guid[]? dietaryRestrictionIds,
            [FromQuery] bool? isAvailable,
            [FromServices] IMediator mediator) =>
        {
            return await mediator.Send(new GetItemsRequest(categoryId, dietaryRestrictionIds, isAvailable));
        })
        .WithName("GetMenuItems")
        .Produces<List<GetItemsResponse>>()
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get all menu items with optional filtering";
            operation.Description = "Retrieve menu items filtered by category, dietary restrictions, and availability. " +
                                  "Multiple dietary restrictions will be combined with AND logic (item must have ALL specified restrictions).";
            return operation;
        });

        group
            .MapGet(
                "/{id:guid}",
                async (Guid id, [FromServices] IMediator mediator) =>
                {
                    return await mediator.Send(new GetItemRequest(id));
                }
            )
            .WithName("GetMenuItemById")
            .Produces<GetItemResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group
            .MapPost(
                "/",
                async ([FromBody] CreateItemRequest command, [FromServices] IMediator mediator) =>
                {
                    return await mediator.Send(command);
                }
            )
            .WithName("CreateMenuItem")
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Kitchen"))
            .Produces<CreateItemResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    Guid id,
                    [FromBody] UpdateItemRequest request,
                    [FromServices] IMediator mediator
                ) =>
                {
                    return await mediator.Send(new UpdateItemCommand(id, request));
                }
            )
            .WithName("UpdateMenuItem")
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Kitchen"))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group
            .MapDelete(
                "/{id:guid}",
                async (Guid id, [FromServices] IMediator mediator) =>
                {
                    return await mediator.Send(new DeleteItemRequest(id));
                }
            )
            .WithName("DeleteMenuItem")
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Kitchen"))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }
}
