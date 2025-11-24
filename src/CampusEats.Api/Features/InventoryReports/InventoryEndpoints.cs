using CampusEats.Api.Features.InventoryReports.Manage;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CampusEats.Api.Features.InventoryReports;

public static class InventoryEndpoints
{
    public static void MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inventory")
            .WithTags("Inventory");

        group.MapPost("/", async (CreateInventoryItemCommand command, IMediator mediator) =>
            await mediator.Send(command))
            .WithName("CreateInventoryItem")
            .Produces<InventoryItemDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/", async (IMediator mediator) =>
            await mediator.Send(new GetInventoryItemsQuery()))
            .WithName("GetInventoryItems")
            .WithDescription("Daily Inventory Report")
            .Produces<List<InventoryItemDto>>(StatusCodes.Status200OK);

        group.MapPatch("/{id:guid}/quantity", async (Guid id, decimal quantity, IMediator mediator) =>
            await mediator.Send(new UpdateInventoryItemQuantityCommand(id, quantity)))
            .WithName("UpdateInventoryItemQuantity")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
            await mediator.Send(new DeleteInventoryItemCommand(id)))
            .WithName("DeleteInventoryItem")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
