using MediatR;
using Microsoft.AspNetCore.Http;

namespace CampusEats.Api.Features.LoyaltyPoints.Claims;

public record GetClaimsRequest(string UserId) : IRequest<IResult>;

public class ClaimResponse
{
    public Guid ClaimId { get; set; }
    public Guid RewardId { get; set; }
    public string RewardName { get; set; } = null!;
    public DateTimeOffset ClaimedAt { get; set; }
    public string? Notes { get; set; }
}
