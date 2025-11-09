using CampusEats.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Menu;

public static class GetMenuItems
{
    public record Query : IRequest<List<Response>>;

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

    public class Handler : IRequestHandler<Query, List<Response>>
    {
        private readonly CampusDbContext _context;

        public Handler(CampusDbContext context)
        {
            _context = context;
        }

        public async Task<List<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await _context.MenuItems
                .Include(m => m.Category)
                .Select(m => new Response(
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
}

