using CampusEats.Api.Common;
using MediatR;

namespace CampusEats.Api.Features.Users.Overview;

public record GetUsersRequest : IRequest<IResult>;