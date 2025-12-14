using CampusEats.Api.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.LoyaltyPoints.GetRewards;

public class GetRewardsHandler : IRequestHandler<GetRewardsRequest, IResult>
{
    private readonly CampusDbContext _context;

    public GetRewardsHandler(CampusDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(
        GetRewardsRequest request,
        CancellationToken cancellationToken
    )
    {
        var now = DateTimeOffset.UtcNow;

        var rewardsList = await _context
            .LoyaltyRewards.Where(r =>
                r.IsActive
                && (r.ValidFrom == null || r.ValidFrom <= now)
                && (r.ValidUntil == null || r.ValidUntil >= now)
            )
            .ToListAsync(cancellationToken);

        var rewards = rewardsList
            .Select(r => new RewardResponse
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                PointsCost = r.PointsCost,
                DiscountValue = r.DiscountValue,
                MenuItemId = r.MenuItemId,
                IsActive = r.IsActive,
                ValidFrom = r.ValidFrom,
                ValidUntil = r.ValidUntil,
                MinimumTier = r.MinimumTier,
            })
            .ToList();

        return Results.Ok(rewards);
    }
}
