using CampusEats.Api.Data;
using CampusEats.Api.Features.Menu;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.DietaryRestrictions;

public record GetDietaryRestrictionsRequest : IRequest<IResult>;

public class GetDietaryRestrictionsHandler : IRequestHandler<GetDietaryRestrictionsRequest, IResult>
{
    private readonly CampusDbContext _context;

    public GetDietaryRestrictionsHandler(CampusDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(
        GetDietaryRestrictionsRequest request,
        CancellationToken cancellationToken
    )
    {
        var restrictions = await _context
            .DietaryRestrictions.Select(dr => new DietaryRestrictionDto(
                dr.Id,
                dr.Name,
                dr.Description,
                dr.Icon
            ))
            .ToListAsync(cancellationToken);
        return Results.Ok(restrictions);
    }
}
