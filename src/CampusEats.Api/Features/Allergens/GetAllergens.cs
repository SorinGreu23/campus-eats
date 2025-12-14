using CampusEats.Api.Data;
using CampusEats.Api.Features.Menu;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Allergens;

public record GetAllergensRequest : IRequest<IResult>;

public class GetAllergensHandler : IRequestHandler<GetAllergensRequest, IResult>
{
    private readonly CampusDbContext _context;

    public GetAllergensHandler(CampusDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(
        GetAllergensRequest request,
        CancellationToken cancellationToken
    )
    {
        var allergens = await _context
            .Allergens.Select(a => new AllergenDto(a.Id, a.Name, a.Description, a.Icon))
            .ToListAsync(cancellationToken);
        return Results.Ok(allergens);
    }
}
