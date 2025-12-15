using CampusEats.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Menu.Categories;

public class GetCategoriesHandler : IRequestHandler<GetCategoriesRequest, IResult>
{
    private readonly CampusDbContext _db;

    public GetCategoriesHandler(CampusDbContext db)
    {
        _db = db;
    }

    public async Task<IResult> Handle(GetCategoriesRequest request, CancellationToken cancellationToken)
    {
        var categories = await _db.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.DisplayOrder,
                c.IsActive
            })
            .ToListAsync(cancellationToken);

        return Results.Ok(categories);
    }
}
