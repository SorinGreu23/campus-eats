// C#
using MediatR;
using CampusEats.Api.Common;

namespace CampusEats.Api.Features.Users.Login;

public record LoginRequest(string Email, string Password) : IRequest<Result<LoginResponse>>;