using CampusEats.Api.Features.Orders.Create;
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

        return app;
    }
}