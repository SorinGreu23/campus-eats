using CampusEats.Api.Common;
using MediatR;

namespace CampusEats.Api.Features.Users.Get;

public record GetUserRequest(string Email) : IRequest<IResult>;