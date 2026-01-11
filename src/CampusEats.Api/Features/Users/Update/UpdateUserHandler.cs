﻿using CampusEats.Api.Data.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CampusEats.Api.Features.Users.Update;

public class UpdateUserHandler : IRequestHandler<UpdateUserRequest, IResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<UpdateUserRequest> _validator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UpdateUserHandler(
        UserManager<ApplicationUser> userManager,
        IValidator<UpdateUserRequest> validator,
        IHttpContextAccessor httpContextAccessor
    )
    {
        _userManager = userManager;
        _validator = validator;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IResult> Handle(
        UpdateUserRequest request,
        CancellationToken cancellationToken
    )
    {
        var validationError = await ValidateRequestAsync(request, cancellationToken);
        if (validationError is not null)
            return validationError;

        var httpContext = _httpContextAccessor.HttpContext;
        var routeId = httpContext?.Request?.RouteValues["id"] as string;
        if (string.IsNullOrWhiteSpace(routeId))
            return Results.BadRequest("Missing user id in route.");

        var user = await _userManager.FindByIdAsync(routeId);
        if (user is null)
            return Results.BadRequest("User not found");

        var authResult = await AuthorizeUserAsync(httpContext, routeId);
        if (authResult.Error is not null)
            return authResult.Error;

        UpdateUserProperties(user, request);

        var roleError = await HandleRoleChangeAsync(user, request.Role, authResult.IsAdmin);
        if (roleError is not null)
            return roleError;

        var passwordError = await HandlePasswordChangeAsync(user, request, authResult.IsAdmin);
        if (passwordError is not null)
            return passwordError;

        var updateError = await SaveUserAsync(user);
        if (updateError is not null)
            return updateError;

        return BuildSuccessResponse(user);
    }

    private async Task<IResult?> ValidateRequestAsync(
        UpdateUserRequest request,
        CancellationToken cancellationToken
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))
            );
        }
        return null;
    }

    private async Task<AuthorizationResult> AuthorizeUserAsync(HttpContext? httpContext, string routeId)
    {
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
            return new AuthorizationResult(Results.Unauthorized(), false);

        var currentUserId = _userManager.GetUserId(httpContext.User);
        var currentUser = await _userManager.GetUserAsync(httpContext.User);
        var isAdmin = currentUser is not null && await _userManager.IsInRoleAsync(currentUser, "Admin");

        if (currentUserId != routeId && !isAdmin)
            return new AuthorizationResult(Results.Forbid(), isAdmin);

        return new AuthorizationResult(null, isAdmin);
    }

    private static void UpdateUserProperties(ApplicationUser user, UpdateUserRequest request)
    {
        user.FirstName = !string.IsNullOrEmpty(request.FirstName)
            ? request.FirstName
            : throw new InvalidOperationException("First name cannot be empty.");

        user.LastName = !string.IsNullOrEmpty(request.LastName)
            ? request.LastName
            : throw new InvalidOperationException("Last name cannot be empty.");

        user.IsActive = request.IsActive
            ?? throw new InvalidOperationException("IsActive status must be specified.");

        user.UserName = !string.IsNullOrEmpty(request.Username)
            ? request.Username
            : throw new InvalidOperationException("Username cannot be empty.");
    }

    private async Task<IResult?> HandleRoleChangeAsync(
        ApplicationUser user,
        string? newRole,
        bool isAdmin
    )
    {
        if (string.IsNullOrEmpty(newRole))
            return null;

        if (!isAdmin)
            return Results.Forbid();

        var existingRoles = await _userManager.GetRolesAsync(user);
        if (existingRoles.Count > 0)
            await _userManager.RemoveFromRolesAsync(user, existingRoles);

        var roleResult = await _userManager.AddToRoleAsync(user, newRole);
        return roleResult.Succeeded
            ? null
            : Results.BadRequest(string.Join(", ", roleResult.Errors.Select(e => e.Description)));
    }

    private async Task<IResult?> HandlePasswordChangeAsync(
        ApplicationUser user,
        UpdateUserRequest request,
        bool isAdmin
    )
    {
        if (string.IsNullOrEmpty(request.NewPassword))
            return null;

        return isAdmin
            ? await ResetPasswordAsAdminAsync(user, request.NewPassword)
            : await ChangePasswordAsOwnerAsync(user, request.CurrentPassword, request.NewPassword);
    }

    private async Task<IResult?> ResetPasswordAsAdminAsync(ApplicationUser user, string newPassword)
    {
        var removed = await _userManager.RemovePasswordAsync(user);
        if (!removed.Succeeded)
            return Results.BadRequest(string.Join(", ", removed.Errors.Select(e => e.Description)));

        var added = await _userManager.AddPasswordAsync(user, newPassword);
        return added.Succeeded
            ? null
            : Results.BadRequest(string.Join(", ", added.Errors.Select(e => e.Description)));
    }

    private async Task<IResult?> ChangePasswordAsOwnerAsync(
        ApplicationUser user,
        string? currentPassword,
        string newPassword
    )
    {
        if (string.IsNullOrEmpty(currentPassword))
            return Results.BadRequest("Current password is required to change your password.");

        var changed = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return changed.Succeeded
            ? null
            : Results.BadRequest(string.Join(", ", changed.Errors.Select(e => e.Description)));
    }

    private async Task<IResult?> SaveUserAsync(ApplicationUser user)
    {
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
            ? null
            : Results.BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    private static IResult BuildSuccessResponse(ApplicationUser user)
    {
        if (string.IsNullOrEmpty(user.Email))
            return Results.Problem("User email cannot be empty.", statusCode: 500);

        var response = new UpdateUserResponse(
            user.Id,
            user.Email,
            user.UserName ?? string.Empty,
            user.FirstName ?? string.Empty,
            user.LastName ?? string.Empty,
            user.IsActive
        );

        return Results.Ok(response);
    }

    private sealed record AuthorizationResult(IResult? Error, bool IsAdmin);
}
