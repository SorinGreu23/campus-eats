using CampusEats.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Menu;

public static class GetMenuItemById
{
    public record Query(Guid Id) : IRequest<Response?>;

    public record Response(
        Guid Id,
        string Name,
        string? Description,
        decimal Price,
        Guid? CategoryId,
        string? CategoryName,
        string? ImageUrl,
        int? PreparationTimeMinutes,
        bool IsAvailable,
        int? Calories,
        DateTimeOffset? CreatedAt,
        DateTimeOffset? UpdatedAt
    );

    public class Handler : IRequestHandler<Query, Response?>
    {
        private readonly CampusDbContext _context;

        public Handler(CampusDbContext context)
        {
            _context = context;
        }

        public async Task<Response?> Handle(Query request, CancellationToken cancellationToken)
        {
            var menuItem = await _context.MenuItems
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (menuItem == null)
                return null;

            return new Response(
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
}

