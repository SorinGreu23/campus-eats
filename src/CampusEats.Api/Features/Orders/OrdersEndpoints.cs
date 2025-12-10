using CampusEats.Api.Features.Kitchen;
using CampusEats.Api.Features.Orders.Create;
using CampusEats.Api.Features.Orders.Get;
using CampusEats.Api.Features.Orders.Cancel;
using CampusEats.Api.Features.Orders.UpdateStatus;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CampusEats.Api.Features.Orders;

public static class OrdersEndpoints
{
    public static IEndpointRouteBuilder MapOrdersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders");

        group.MapPost("/", async ([FromBody] CreateOrderRequest request, [FromServices] IMediator mediator) =>
                await mediator.Send(request)
            )
            .WithName("CreateOrder")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithOpenApi();

        group.MapGet("/user/{userId}", async ([FromRoute] string userId, [FromServices] IMediator mediator) =>
            {
                var req = new GetOrdersByUserRequest { UserId = userId };
                return await mediator.Send(req);
            })
            .WithName("GetOrdersByUser")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        app.MapGet("/pending", async (IMediator mediator) =>
                await mediator.Send(new GetPendingOrdersQuery()))
            .WithName("GetPendingOrders")
            .WithTags("Kitchen")
            .WithDescription("Returns all orders that are in Pending or Preparing status")
            .Produces<List<PendingOrderDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPatch("/{orderId:guid}/cancel", async ([FromRoute] Guid orderId, [FromBody] CancelOrderRequest? body,
                [FromServices] IMediator mediator) =>
            {
                var req = new CancelOrderRequest { Reason = body?.Reason };
                return await mediator.Send(req);
            })
            .WithName("CancelOrder")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        group.MapPatch("/status", async ([FromBody] UpdateOrderStatusRequest request, IMediator mediator) =>
            await mediator.Send(request)
        );

        return app;
    }
}