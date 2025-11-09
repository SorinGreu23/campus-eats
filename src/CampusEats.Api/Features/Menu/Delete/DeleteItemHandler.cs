using CampusEats.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Menu;

public class DeleteItemHandler : IRequestHandler<DeleteItemRequest, bool>
{
    private readonly CampusDbContext _context;

    public DeleteItemHandler(CampusDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteItemRequest request, CancellationToken cancellationToken)
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

