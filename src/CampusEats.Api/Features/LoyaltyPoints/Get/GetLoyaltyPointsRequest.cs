using MediatR;
using Microsoft.AspNetCore.Http;

namespace CampusEats.Api.Features.LoyaltyPoints.Get;

public record GetLoyaltyPointsRequest(string UserId) : IRequest<IResult>;

public class LoyaltyPointsResponse
{
    public Guid AccountId { get; set; }
    public string UserId { get; set; } = null!;
    public int PointsBalance { get; set; }
    public int LifetimePoints { get; set; }
    public string Tier { get; set; } = null!;
}
