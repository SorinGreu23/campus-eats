using CampusEats.Api.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Menu;

public class DeleteItemHandler : IRequestHandler<DeleteItemRequest, IResult>
{
    private readonly CampusDbContext _context;

    public DeleteItemHandler(CampusDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(DeleteItemRequest request, CancellationToken cancellationToken)
    {
        var menuItem = await _context.MenuItems
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (menuItem == null)
        {
            return Results.NotFound(new { message = $"Menu item with ID '{request.Id}' was not found." });
        }

        _context.MenuItems.Remove(menuItem);
        await _context.SaveChangesAsync(cancellationToken);
        
        return Results.NoContent();
    }
}

