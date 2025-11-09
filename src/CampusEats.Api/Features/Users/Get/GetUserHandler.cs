using CampusEats.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Users.Get;

public class GetUserHandler : IRequestHandler<GetUserRequest, GetUserResponse>
{
    private readonly CampusDbContext _context;

    public GetUserHandler(CampusDbContext context)
    {
        _context = context;
    }

    public async Task<GetUserResponse> Handle(GetUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {request.Id} not found");
        }

        return new GetUserResponse(
            user.Id,
            user.Email,
            user.FirstName ?? string.Empty,
            user.LastName ?? string.Empty,
            user.Role,
            user.IsActive,
            user.CreatedAt!.Value,
            user.UpdatedAt
        );
    }
}