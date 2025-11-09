using CampusEats.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Menu;

public class GetItemsHandler : IRequestHandler<GetItemsRequest, List<GetItemsResponse>>
{
    private readonly CampusDbContext _context;

    public GetItemsHandler(CampusDbContext context)
    {
        _context = context;
    }

    public async Task<List<GetItemsResponse>> Handle(GetItemsRequest request, CancellationToken cancellationToken)
    {
        return await _context.MenuItems
            .Include(m => m.Category)
            .Select(m => new GetItemsResponse(
                m.Id,
                m.Name,
                m.Description,
                m.Price,
                m.CategoryId,
                m.Category != null ? m.Category.Name : null,
                m.ImageUrl,
                m.PreparationTimeMinutes,
                m.IsAvailable,
                m.Calories,
                m.CreatedAt,
                m.UpdatedAt
            ))
            .ToListAsync(cancellationToken);
    }
}

