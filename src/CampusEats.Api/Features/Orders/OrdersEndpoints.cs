using CampusEats.Api.Features.Orders.Create;
using CampusEats.Api.Features.Orders.Get;
using CampusEats.Api.Features.Orders.Cancel;
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
        {
            return await mediator.Send(request);
        })
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
        
        group.MapPatch("/{orderId:guid}/cancel", async ([FromRoute] Guid orderId, [FromBody] CancelOrderRequest? body, [FromServices] IMediator mediator) =>
        {
            var req = new CancelOrderRequest { Reason = body?.Reason };
            return await mediator.Send(req);
        })
        .WithName("CancelOrder")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .WithOpenApi();

        return app;
    }
}