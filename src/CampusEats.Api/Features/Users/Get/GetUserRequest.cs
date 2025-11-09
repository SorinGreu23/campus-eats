using MediatR;

namespace CampusEats.Api.Features.Users.Get;

public record GetUserRequest(Guid Id) : IRequest<GetUserResponse>;