using CampusEats.Api.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.LoyaltyPoints.Claims;

public class GetClaimsHandler : IRequestHandler<GetClaimsRequest, IResult>
{
    private readonly CampusDbContext _context;

    public GetClaimsHandler(CampusDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(GetClaimsRequest request, CancellationToken cancellationToken)
    {
        var account = await _context.LoyaltyAccounts.FirstOrDefaultAsync(
            a => a.UserId == request.UserId,
            cancellationToken
        );
        if (account == null)
            return Results.Ok(new List<ClaimResponse>());

        var claims = await _context
            .LoyaltyClaims
            .Include(c => c.LoyaltyReward)
            .Where(c => c.LoyaltyAccountId == account.Id)
            .OrderByDescending(c => c.ClaimedAt)
            .Select(c => new ClaimResponse
            {
                Id = c.Id,
                UserId = request.UserId,
                RewardId = c.RewardId,
                Reward = c.LoyaltyReward == null ? null : new RewardDto
                {
                    Id = c.LoyaltyReward.Id,
                    Name = c.LoyaltyReward.Name,
                    Description = c.LoyaltyReward.Description,
                    PointsCost = c.LoyaltyReward.PointsCost,
                    DiscountValue = c.LoyaltyReward.DiscountValue,
                    IsActive = c.LoyaltyReward.IsActive
                },
                RedeemedAt = c.ClaimedAt,
                ExpiresAt = null,
                IsUsed = c.Notes == "Used",
                UsedAt = c.Notes == "Used" ? c.ClaimedAt : null,
                OrderId = null
            })
            .ToListAsync(cancellationToken);

        return Results.Ok(claims);
    }
}
