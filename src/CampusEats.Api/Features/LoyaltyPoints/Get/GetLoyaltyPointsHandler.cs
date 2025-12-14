using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.LoyaltyPoints.Get;

public class GetLoyaltyPointsHandler : IRequestHandler<GetLoyaltyPointsRequest, IResult>
{
    private readonly CampusDbContext _context;

    public GetLoyaltyPointsHandler(CampusDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(
        GetLoyaltyPointsRequest request,
        CancellationToken cancellationToken
    )
    {
        var loyaltyAccount = await _context.LoyaltyAccounts.FirstOrDefaultAsync(
            la => la.UserId == request.UserId,
            cancellationToken
        );

        if (loyaltyAccount == null)
        {
            // Create a new loyalty account if it doesn't exist
            loyaltyAccount = new LoyaltyAccount
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                PointsBalance = 0,
                LifetimePoints = 0,
                Tier = "Bronze",
            };
            _context.LoyaltyAccounts.Add(loyaltyAccount);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var response = new LoyaltyPointsResponse
        {
            AccountId = loyaltyAccount.Id,
            UserId = loyaltyAccount.UserId!,
            PointsBalance = loyaltyAccount.PointsBalance,
            LifetimePoints = loyaltyAccount.LifetimePoints,
            Tier = loyaltyAccount.Tier ?? "Bronze",
        };

        return Results.Ok(response);
    }
}
