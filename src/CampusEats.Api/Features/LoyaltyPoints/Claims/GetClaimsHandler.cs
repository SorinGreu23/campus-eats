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
        var account = await _context.LoyaltyAccounts.FirstOrDefaultAsync(a => a.UserId == request.UserId, cancellationToken);
        if (account == null) return Results.Ok(new List<ClaimResponse>());

        var claims = await _context.LoyaltyClaims
            .Where(c => c.LoyaltyAccountId == account.Id)
            .OrderByDescending(c => c.ClaimedAt)
            .Join(_context.LoyaltyRewards,
                c => c.RewardId,
                r => r.Id,
                (c, r) => new ClaimResponse
                {
                    ClaimId = c.Id,
                    RewardId = r.Id,
                    RewardName = r.Name,
                    ClaimedAt = c.ClaimedAt,
                    Notes = c.Notes
                })
            .ToListAsync(cancellationToken);

        return Results.Ok(claims);
    }
}
