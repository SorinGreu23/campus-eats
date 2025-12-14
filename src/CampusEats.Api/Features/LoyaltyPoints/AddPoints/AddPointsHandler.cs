using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.LoyaltyPoints.AddPoints;

public class AddPointsHandler : IRequestHandler<AddPointsRequest, IResult>
{
    private readonly CampusDbContext _context;

    public AddPointsHandler(CampusDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(AddPointsRequest request, CancellationToken cancellationToken)
    {
        if (request.Points <= 0)
        {
            return Results.BadRequest("Points must be greater than zero.");
        }

        // Get or create loyalty account
        var loyaltyAccount = await _context.LoyaltyAccounts.FirstOrDefaultAsync(
            la => la.UserId == request.UserId,
            cancellationToken
        );

        if (loyaltyAccount == null)
        {
            loyaltyAccount = new LoyaltyAccount
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                PointsBalance = 0,
                LifetimePoints = 0,
                Tier = "Bronze",
            };
            _context.LoyaltyAccounts.Add(loyaltyAccount);
        }

        // Add points
        loyaltyAccount.PointsBalance += request.Points;
        loyaltyAccount.LifetimePoints += request.Points;

        // Update tier based on lifetime points
        loyaltyAccount.Tier = CalculateTier(loyaltyAccount.LifetimePoints);

        await _context.SaveChangesAsync(cancellationToken);

        var response = new AddPointsResponse
        {
            AccountId = loyaltyAccount.Id,
            PointsAdded = request.Points,
            NewPointsBalance = loyaltyAccount.PointsBalance,
            LifetimePoints = loyaltyAccount.LifetimePoints,
            Tier = loyaltyAccount.Tier,
        };

        return Results.Ok(response);
    }

    private static string CalculateTier(int lifetimePoints)
    {
        return lifetimePoints switch
        {
            >= 10000 => "Platinum",
            >= 5000 => "Gold",
            >= 1000 => "Silver",
            _ => "Bronze",
        };
    }
}
