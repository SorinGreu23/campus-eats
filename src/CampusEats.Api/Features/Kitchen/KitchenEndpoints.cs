using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Kitchen;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CampusEats.Api.Features.Kitchen;

public static class KitchenEndpoints
{
    public static void MapKitchenEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/kitchen/pending-orders", async (IMediator mediator) =>
            await mediator.Send(new GetPendingOrdersQuery()))
            .WithName("GetPendingOrders")
            .WithTags("Kitchen")
            .WithDescription("Returns all orders that are in Pending or Preparing status")
            .Produces<List<PendingOrderDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        app.MapPut("/api/kitchen/orders/{id:guid}/status", async (Guid id, string status, IMediator mediator) =>
        {
            var orderStatus = Enum.Parse<OrderStatus>(status);
            var command = new UpdateOrderStatusCommand(id, orderStatus);
            return await mediator.Send(command);
        })
            .WithName("UpdateOrderStatus")
            .WithTags("Kitchen")
            .WithDescription("Updates the status of an order. Valid transitions: Pending → Preparing → Ready → Completed")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
