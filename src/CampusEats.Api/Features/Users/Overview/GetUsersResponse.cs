namespace CampusEats.Api.Features.Users.Overview;

public record GetUsersResponse(List<UserDto> Users);

public record UserDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool IsActive
);
