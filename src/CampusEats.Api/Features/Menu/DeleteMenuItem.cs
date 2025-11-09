using CampusEats.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Menu;

public static class DeleteMenuItem
{
    public record Command(Guid Id) : IRequest<bool>;

    public class Handler : IRequestHandler<Command, bool>
    {
        private readonly CampusDbContext _context;

        public Handler(CampusDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            var menuItem = await _context.MenuItems
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (menuItem == null)
                return false;

            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}

