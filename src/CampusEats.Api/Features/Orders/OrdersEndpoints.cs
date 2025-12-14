using System.Security.Claims;
using CampusEats.Api.Data.Entities;
using CampusEats.Api.Features.Kitchen;
using CampusEats.Api.Features.Orders.Cancel;
using CampusEats.Api.Features.Orders.Create;
using CampusEats.Api.Features.Orders.Get;
using CampusEats.Api.Features.Orders.Complete;
using CampusEats.Api.Features.Orders.Pending;
using CampusEats.Api.Features.Orders.UpdateStatus;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CampusEats.Api.Features.Orders;

public static class OrdersEndpoints
{
    public static IEndpointRouteBuilder MapOrdersEndpoints(this IEndpointRouteBuilder app)
    {
        // Public kitchen-facing endpoint for pending orders (no auth enforced)
        app.MapGet("/api/orders/pending", async ([FromServices] IMediator mediator) => 
            await mediator.Send(new GetPendingOrdersRequest()))
        .WithTags("Orders")
        .WithName("GetPendingOrders")
        .AllowAnonymous()
        .Produces(StatusCodes.Status200OK)
        .WithOpenApi();

        var group = app.MapGroup("/api/orders")
            .WithTags("Orders")
            .RequireAuthorization();

        group.MapPost("/", async (
            HttpContext httpContext,
            [FromBody] CreateOrderRequest request, 
            [FromServices] IMediator mediator,
            [FromServices] UserManager<ApplicationUser> userManager) =>
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return Results.NotFound(new { error = "User not found." });
            }

            // Set the userId from the authenticated user
            request.UserId = userId;
            return await mediator.Send(request);
        })
        .WithName("CreateOrder")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .WithOpenApi();
        
        group.MapGet("/user/me", async (
            HttpContext httpContext,
            [FromServices] IMediator mediator,
            [FromServices] UserManager<ApplicationUser> userManager) =>
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return Results.NotFound(new { error = "User not found." });
            }

            var req = new GetOrdersByUserRequest { UserId = userId };
            return await mediator.Send(req);
        })
        .WithName("GetOrdersByUser")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .WithOpenApi();

        group
            .MapPatch(
                "/{orderId:guid}/cancel",
                async (
                    [FromRoute] Guid orderId,
                    [FromBody] CancelOrderRequest? body,
                    [FromServices] IMediator mediator
                ) =>
                {
                    var req = new CancelOrderRequest { OrderId = orderId, Reason = body?.Reason };
                    return await mediator.Send(req);
                }
            )
            .WithName("CancelOrder")
            .RequireAuthorization() // Admin/Kitchen or owner enforced in handler
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        group.MapPatch("/{orderId:guid}/complete", async ([FromRoute] Guid orderId, [FromServices] IMediator mediator) =>
        {
            var req = new CompleteOrderRequest(orderId);
            return await mediator.Send(req);
        })
        .WithName("CompleteOrder")
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
