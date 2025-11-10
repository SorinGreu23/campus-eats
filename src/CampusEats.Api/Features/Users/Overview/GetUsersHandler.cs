using CampusEats.Api.Common;
using CampusEats.Api.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Users.Overview;

public class GetUsersHandler : IRequestHandler<GetUsersRequest, Result<GetUsersResponse>>
{
    private readonly CampusDbContext _context;

    public GetUsersHandler(CampusDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GetUsersResponse>> Handle(GetUsersRequest request, CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserDto(
                u.Id,
                u.Email,
                u.FirstName ?? string.Empty,
                u.LastName ?? string.Empty,
                u.Role ?? "Customer",
                u.IsActive,
                u.CreatedAt!.Value
            ))
            .ToListAsync(cancellationToken);

        return Result<GetUsersResponse>.Success(new GetUsersResponse(users));
    }
}