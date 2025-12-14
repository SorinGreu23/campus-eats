using CampusEats.Api.Features.LoyaltyPoints.AddPoints;
using CampusEats.Api.Features.LoyaltyPoints.Claims;
using CampusEats.Api.Features.LoyaltyPoints.Get;
using CampusEats.Api.Features.LoyaltyPoints.GetRewards;
using CampusEats.Api.Features.LoyaltyPoints.RedeemReward;
using MediatR;

namespace CampusEats.Api.Features.LoyaltyPoints;

public static class LoyaltyPointsEndpoints
{
    public static void MapLoyaltyPointsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/loyalty").WithTags("Loyalty Points");

        group
            .MapGet(
                "/account/{userId}",
                async (string userId, IMediator mediator) =>
                    await mediator.Send(new GetLoyaltyPointsRequest(userId))
            )
            .WithName("GetLoyaltyAccount")
            .WithOpenApi();

        group
            .MapGet(
                "/rewards",
                async (IMediator mediator) => await mediator.Send(new GetRewardsRequest())
            )
            .WithName("GetLoyaltyRewards")
            .WithOpenApi();

        group
            .MapPost(
                "/points",
                async (AddPointsRequest request, IMediator mediator) => await mediator.Send(request)
            )
            .WithName("AddLoyaltyPoints")
            .WithOpenApi();

        group
            .MapPost(
                "/redeem",
                async (RedeemRewardRequest request, IMediator mediator) =>
                    await mediator.Send(request)
            )
            .WithName("RedeemLoyaltyReward")
            .WithOpenApi();

        group
            .MapGet(
                "/claims/{userId}",
                async (string userId, IMediator mediator) =>
                    await mediator.Send(new GetClaimsRequest(userId))
            )
            .WithName("GetLoyaltyClaims")
            .WithOpenApi();
    }
}
