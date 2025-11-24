using CampusEats.Api.Data;
using MediatR;

namespace CampusEats.Api.Features.InventoryReports.Manage;

public record DeleteInventoryItemCommand(Guid Id) : IRequest<IResult>;

public class DeleteInventoryItemHandler : IRequestHandler<DeleteInventoryItemCommand, IResult>
{
    private readonly CampusDbContext _context;

    public DeleteInventoryItemHandler(CampusDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle(DeleteInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.InventoryItems.FindAsync(new object[] { request.Id }, cancellationToken);
        if (item == null)
        {
            return Results.NotFound();
        }

        _context.InventoryItems.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}
