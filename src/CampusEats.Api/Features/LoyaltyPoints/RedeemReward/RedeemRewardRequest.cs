using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CampusEats.Api.Features.LoyaltyPoints.RedeemReward;

public class RedeemRewardRequest : IRequest<IResult>
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = null!;
    
    [JsonPropertyName("rewardId")]
    public Guid RewardId { get; set; }
}

public class RedeemRewardResponse
{
    [JsonPropertyName("accountId")]
    public Guid AccountId { get; set; }
    
    [JsonPropertyName("newPointsBalance")]
    public int NewPointsBalance { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = null!;
}

