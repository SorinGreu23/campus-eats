using CampusEats.Api.Features.Inventory.Get;
using CampusEats.Api.Features.Inventory.GetById;
using CampusEats.Api.Features.Inventory.Restock;
using CampusEats.Api.Features.Inventory.Use;
using MediatR;

namespace CampusEats.Api.Features.Inventory;

public static class InventoryEndpoints
{
    public static void MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inventory")
            .WithTags("Inventory");

        group.MapGet("/", async (IMediator mediator) =>
        {
            var request = new GetInventoryItemsRequest();
            return await mediator.Send(request);
        })
        .WithName("GetAllInventoryItems")
        .WithOpenApi();

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var request = new GetInventoryItemRequest(id);
            return await mediator.Send(request);
        })
        .WithName("GetInventoryItemById")
        .WithOpenApi();

        group.MapPost("/{id:guid}/restock", async (Guid id, RestockRequest body, IMediator mediator) =>
        {
            var request = new RestockInventoryRequest(id, body.Quantity, body.Reason);
            return await mediator.Send(request);
        })
        .WithName("RestockInventoryItem")
        .WithOpenApi();

        group.MapPost("/{id:guid}/use", async (Guid id, UseRequest body, IMediator mediator) =>
        {
            var request = new UseInventoryRequest(id, body.Quantity, body.Reason);
            return await mediator.Send(request);
        })
        .WithName("UseInventoryItem")
        .WithOpenApi();
    }

    private record RestockRequest(decimal Quantity, string? Reason);
    private record UseRequest(decimal Quantity, string? Reason);
}
