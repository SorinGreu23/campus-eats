using CampusEats.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Menu;

public class GetItemHandler : IRequestHandler<GetItemRequest, GetItemResponse?>
{
    private readonly CampusDbContext _context;

    public GetItemHandler(CampusDbContext context)
    {
        _context = context;
    }

    public async Task<GetItemResponse?> Handle(GetItemRequest request, CancellationToken cancellationToken)
    {
        var menuItem = await _context.MenuItems
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (menuItem == null)
            return null;

        return new GetItemResponse(
            menuItem.Id,
            menuItem.Name,
            menuItem.Description,
            menuItem.Price,
            menuItem.CategoryId,
            menuItem.Category?.Name,
            menuItem.ImageUrl,
            menuItem.PreparationTimeMinutes,
            menuItem.IsAvailable,
            menuItem.Calories,
            menuItem.CreatedAt,
            menuItem.UpdatedAt
        );
    }
}

