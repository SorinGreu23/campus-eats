using MediatR;
using Microsoft.AspNetCore.Http;

namespace CampusEats.Api.Features.Users.Overview;

public record GetUsersQuery : IRequest<IResult>;