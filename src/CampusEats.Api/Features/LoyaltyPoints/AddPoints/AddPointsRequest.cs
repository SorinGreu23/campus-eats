using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CampusEats.Api.Features.LoyaltyPoints.AddPoints;

public class AddPointsRequest : IRequest<IResult>
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = null!;
    
    [JsonPropertyName("points")]
    public int Points { get; set; }
    
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}

public class AddPointsResponse
{
    [JsonPropertyName("accountId")]
    public Guid AccountId { get; set; }
    
    [JsonPropertyName("pointsAdded")]
    public int PointsAdded { get; set; }
    
    [JsonPropertyName("newPointsBalance")]
    public int NewPointsBalance { get; set; }
    
    [JsonPropertyName("lifetimePoints")]
    public int LifetimePoints { get; set; }
    
    [JsonPropertyName("tier")]
    public string Tier { get; set; } = null!;
}

