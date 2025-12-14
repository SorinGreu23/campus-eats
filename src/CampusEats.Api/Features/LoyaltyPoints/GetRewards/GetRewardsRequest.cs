using MediatR;
using Microsoft.AspNetCore.Http;

namespace CampusEats.Api.Features.LoyaltyPoints.GetRewards;

public record GetRewardsRequest() : IRequest<IResult>;

public class RewardResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int PointsCost { get; set; }
    public decimal? DiscountValue { get; set; }
    public Guid? MenuItemId { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset? ValidFrom { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
    public string? MinimumTier { get; set; }
}
