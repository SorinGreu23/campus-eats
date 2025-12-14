// C#
using CampusEats.Api.Common;
using MediatR;

namespace CampusEats.Api.Features.Users.Login;

public record LoginRequest(string Email, string Password) : IRequest<IResult>;
