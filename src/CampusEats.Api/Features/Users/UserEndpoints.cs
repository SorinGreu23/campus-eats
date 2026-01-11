using CampusEats.Api.Features.Users.Create;
using CampusEats.Api.Features.Users.Delete;
using CampusEats.Api.Features.Users.Get;
using CampusEats.Api.Features.Users.Login;
using CampusEats.Api.Features.Users.Overview;
using CampusEats.Api.Features.Users.Update;
using MediatR;

namespace CampusEats.Api.Features.Users;

public static class UserEndpoints
{
  private const string UsersTag = "Users";

  public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/users/register",
                async (RegisterRequest request, IMediator mediator) => await mediator.Send(request)
            )
            .WithName("RegisterUser")
            .WithTags(UsersTag)
            .WithOpenApi();

        app.MapPost(
                "/api/users/login",
                async (LoginRequest request, IMediator mediator) => await mediator.Send(request)
            )
            .WithName("LoginUser")
            .WithTags(UsersTag)
            .WithOpenApi();

        app.MapGet(
                "/api/users",
                async (IMediator mediator) => await mediator.Send(new GetUsersQuery())
            )
            .WithName("GetUsers")
            .WithTags(UsersTag)
            .WithDescription("Lists all users. Admins only.")
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithOpenApi();

        app.MapGet(
                "/api/users/{id}",
                async (string id, IMediator mediator) => await mediator.Send(new GetUserRequest(id))
            )
            .WithName("GetUser")
            .WithTags(UsersTag)
            .WithOpenApi();

        app.MapPut(
                "/api/users/{id}",
                async (string id, UpdateUserRequest request, IMediator mediator) =>
                    await mediator.Send(request)
            )
            .WithName("UpdateUser")
            .WithTags(UsersTag)
            .WithDescription("Updates a user. Only the account owner or an admin can update.")
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        app.MapDelete(
                "/api/users/{id}",
                async (string id, IMediator mediator) =>
                    await mediator.Send(new DeleteUserRequest(id))
            )
            .WithName("DeleteUser")
            .WithTags(UsersTag)
            .WithDescription(
                "Deletes a user account. Only the account owner or an admin can delete the account."
            )
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();
    }
}
