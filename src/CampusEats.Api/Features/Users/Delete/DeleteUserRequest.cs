using CampusEats.Api.Common;
using MediatR;

namespace CampusEats.Api.Features.Users.Delete;

public record DeleteUserRequest(Guid Id) : IRequest<Result<DeleteUserResponse>>;