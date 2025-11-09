using CampusEats.Api.Data;
using CampusEats.Api.Data.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Users.Delete;

public class DeleteUserHandler : IRequestHandler<DeleteUserRequest>
{
    private readonly CampusDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DeleteUserHandler(CampusDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task Handle(DeleteUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {request.Id} not found");
        }

        var appUser = await _userManager.Users
            .FirstOrDefaultAsync(au => au.UserId == user.Id, cancellationToken);

        if (appUser != null)
        {
            await _userManager.DeleteAsync(appUser);
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);
    }
}