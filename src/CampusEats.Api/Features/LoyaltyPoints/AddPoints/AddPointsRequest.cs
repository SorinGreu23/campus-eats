using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CampusEats.Api.Features.LoyaltyPoints.AddPoints;

public class AddPointsRequest : IRequest<IResult>
{
    public string UserId { get; set; } = null!;
    
    public int Points { get; set; }
    
    public string? Reason { get; set; }
}

public class AddPointsResponse
{
    public Guid AccountId { get; set; }
    
    public int PointsAdded { get; set; }
    
    public int NewPointsBalance { get; set; }
    
    public int LifetimePoints { get; set; }

    public string Tier { get; set; } = null!;
}

