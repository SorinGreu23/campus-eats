using MediatR;
using Microsoft.AspNetCore.Mvc;
using CampusEats.Api.Features.Menu.Categories;
using CampusEats.Api.Features.Menu.Ingredients;

namespace CampusEats.Api.Features.Menu;

public static class MenuEndpoints
{
    public static IEndpointRouteBuilder MapMenuEndpoints(this IEndpointRouteBuilder app)
    {
        // Categories endpoint
        app.MapGet("/api/categories", async ([FromServices] IMediator mediator) =>
        {
            return await mediator.Send(new GetCategoriesRequest());
        })
        .WithName("GetCategories")
        .WithTags("Categories")
        .Produces(StatusCodes.Status200OK);

        var group = app.MapGroup("/api/menuitems")
            .WithTags("MenuItems");

        group
            .MapGet(
                "/",
                async (
                    [FromQuery] Guid? categoryId,
                    [FromQuery] Guid[]? dietaryRestrictionIds,
                    [FromQuery] bool? isAvailable,
                    [FromQuery] string? searchTerm,
                    [FromServices] IMediator mediator
                ) =>
                {
                    return await mediator.Send(
                        new GetItemsRequest(categoryId, dietaryRestrictionIds, isAvailable, searchTerm)
                    );
                }
            )
            .WithName("GetMenuItems")
            .Produces<List<GetItemsResponse>>()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get all menu items with optional filtering";
                operation.Description =
                    "Retrieve menu items filtered by category ID, dietary restrictions, availability, and search term. "
                    + "Multiple dietary restrictions will be combined with AND logic (item must have ALL specified restrictions). "
                    + "Search term filters by name and description (case-insensitive).";
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

        // Menu Item Ingredients endpoints
        group
            .MapGet(
                "/{id:guid}/ingredients",
                async (Guid id, [FromServices] IMediator mediator) =>
                {
                    return await mediator.Send(new GetMenuItemIngredientsRequest(id));
                }
            )
            .WithName("GetMenuItemIngredients")
            .Produces(StatusCodes.Status200OK);

        group
            .MapPost(
                "/{menuItemId:guid}/ingredients",
                async (
                    Guid menuItemId,
                    [FromBody] AddIngredientBody body,
                    [FromServices] IMediator mediator
                ) =>
                {
                    return await mediator.Send(new AddMenuItemIngredientRequest(
                        menuItemId,
                        body.InventoryItemId,
                        body.QuantityRequired
                    ));
                }
            )
            .WithName("AddMenuItemIngredient")
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Kitchen"))
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group
            .MapPut(
                "/{menuItemId:guid}/ingredients/{inventoryItemId:guid}",
                async (
                    Guid menuItemId,
                    Guid inventoryItemId,
                    [FromBody] UpdateIngredientBody body,
                    [FromServices] IMediator mediator
                ) =>
                {
                    return await mediator.Send(new UpdateMenuItemIngredientRequest(
                        menuItemId,
                        inventoryItemId,
                        body.QuantityRequired
                    ));
                }
            )
            .WithName("UpdateMenuItemIngredient")
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Kitchen"))
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group
            .MapDelete(
                "/{menuItemId:guid}/ingredients/{inventoryItemId:guid}",
                async (
                    Guid menuItemId,
                    Guid inventoryItemId,
                    [FromServices] IMediator mediator
                ) =>
                {
                    return await mediator.Send(new DeleteMenuItemIngredientRequest(
                        menuItemId,
                        inventoryItemId
                    ));
                }
            )
            .WithName("DeleteMenuItemIngredient")
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Kitchen"))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private record AddIngredientBody(Guid InventoryItemId, decimal QuantityRequired);
    private record UpdateIngredientBody(decimal QuantityRequired);
}
