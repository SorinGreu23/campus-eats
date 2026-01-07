using CampusEats.Api.Features.Payments.ConfirmPayment;
using CampusEats.Api.Features.Payments.CreatePaymentIntent;
using CampusEats.Api.Features.Payments.GetStripeConfig;
using CampusEats.Api.Features.Payments.TestConfirm;
using CampusEats.Api.Features.Payments.WebhookHandler;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CampusEats.Api.Features.Payments;

public static class PaymentsEndpoints
{
    public static IEndpointRouteBuilder MapPaymentsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payments")
            .WithTags("Payments");

        // Get Stripe configuration (publishable key) - requires authentication
        group.MapGet("/config", async ([FromServices] IMediator mediator) =>
        {
            return await mediator.Send(new GetStripeConfigRequest());
        })
        .WithName("GetStripeConfig")
        .RequireAuthorization()
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .WithOpenApi();

        // Create payment intent for an order
        group.MapPost("/create-payment-intent", async (
            [FromBody] CreatePaymentIntentRequest request,
            [FromServices] IMediator mediator) =>
        {
            return await mediator.Send(request);
        })
        .WithName("CreatePaymentIntent")
        .RequireAuthorization()
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .WithOpenApi();

        // Confirm payment after successful Stripe processing
        group.MapPost("/confirm", async (
            [FromBody] ConfirmPaymentRequest request,
            [FromServices] IMediator mediator) =>
        {
            return await mediator.Send(request);
        })
        .WithName("ConfirmPayment")
        .RequireAuthorization()
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .WithOpenApi();

        // TEST ONLY: Confirm payment without Stripe validation (Development)
        group.MapPost("/test-confirm", async (
            [FromBody] TestConfirmPaymentRequest request,
            [FromServices] IMediator mediator,
            [FromServices] IWebHostEnvironment env) =>
        {
            if (!env.IsDevelopment())
            {
                return Results.NotFound();
            }
            return await mediator.Send(request);
        })
        .WithName("TestConfirmPayment")
        .RequireAuthorization()
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .WithOpenApi();

        // Stripe webhook endpoint - no authentication required (validated by signature)
        group.MapPost("/webhook", async (
            HttpContext httpContext,
            [FromServices] IMediator mediator) =>
        {
            var json = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
            var signature = httpContext.Request.Headers["Stripe-Signature"].ToString();

            var request = new StripeWebhookRequest
            {
                Payload = json,
                Signature = signature
            };

            return await mediator.Send(request);
        })
        .WithName("StripeWebhook")
        .AllowAnonymous()
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .WithOpenApi();

        return app;
    }
}
