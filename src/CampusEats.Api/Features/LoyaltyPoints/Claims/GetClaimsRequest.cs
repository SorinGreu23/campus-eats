using MediatR;
using Microsoft.AspNetCore.Http;

namespace CampusEats.Api.Features.LoyaltyPoints.Claims;

public record GetClaimsRequest(string UserId) : IRequest<IResult>;

public class ClaimResponse
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = null!;
    public Guid RewardId { get; set; }
    public RewardDto? Reward { get; set; }
    public DateTimeOffset RedeemedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
    public Guid? OrderId { get; set; }
}

public class RewardDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int PointsCost { get; set; }
    public decimal? DiscountValue { get; set; }
    public bool IsActive { get; set; }
}
