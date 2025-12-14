using CampusEats.Api.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CampusEats.Api.Features.Users.Delete;

public record DeleteUserRequest(string Id) : IRequest<IResult>;
