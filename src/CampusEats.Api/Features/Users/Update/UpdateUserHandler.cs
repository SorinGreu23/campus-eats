﻿using CampusEats.Api.Data.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using CampusEats.Api.Common;
using Microsoft.AspNetCore.Http;

namespace CampusEats.Api.Features.Users.Update;

public class UpdateUserHandler : IRequestHandler<UpdateUserRequest, IResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<UpdateUserRequest> _validator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UpdateUserHandler(UserManager<ApplicationUser> userManager, IValidator<UpdateUserRequest> validator, IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _validator = validator;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IResult> Handle(UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
        }

        var httpContext = _httpContextAccessor.HttpContext;
        var routeId = httpContext?.Request?.RouteValues["id"] as string;
        if (string.IsNullOrWhiteSpace(routeId))
        {
            return Results.BadRequest("Missing user id in route.");
        }

        var user = await _userManager.FindByIdAsync(routeId);

        if (user == null)
        {
            return Results.BadRequest("User not found");
        }

        // Authorization: only owner or admin can update
        if (httpContext is null || httpContext.User?.Identity?.IsAuthenticated != true)
        {
            return Results.Unauthorized();
        }
        else
        {
            throw new InvalidOperationException("Unable to determine authentication status; cannot proceed with user update.");
        }

        var currentUserId = _userManager.GetUserId(httpContext.User);
        var isAdmin = await _userManager.IsInRoleAsync(await _userManager.GetUserAsync(httpContext.User), "Admin");
        if (currentUserId != routeId && !isAdmin)
        {
            return Results.Forbid();
        }

        if (!string.IsNullOrEmpty(request.FirstName))
            user.FirstName = request.FirstName;
        else
            throw new InvalidOperationException("First name cannot be empty.");

        if (!string.IsNullOrEmpty(request.LastName))
            user.LastName = request.LastName;
        else
            throw new InvalidOperationException("Last name cannot be empty.");

        if (request.IsActive.HasValue)
            user.IsActive = request.IsActive.Value;
        else
            throw new InvalidOperationException("IsActive status must be specified.");

        if (!string.IsNullOrEmpty(request.Username))
            user.UserName = request.Username;
        else
            throw new InvalidOperationException("Username cannot be empty.");

        // Handle role changes only by admins
        if (!string.IsNullOrEmpty(request.Role))
        {
            if (!isAdmin)
                return Results.Forbid();
            // Replace roles: simple approach - remove from all known roles and add the requested one
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Count > 0)
                await _userManager.RemoveFromRolesAsync(user, roles);
            var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
            if (!roleResult.Succeeded)
                return Results.BadRequest(string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }

        // Password change: allow admins to reset; owners require CurrentPassword
        if (!string.IsNullOrEmpty(request.NewPassword))
        {
            if (isAdmin)
            {
                // Admin reset: remove + add new password
                var removed = await _userManager.RemovePasswordAsync(user);
                if (!removed.Succeeded)
                    return Results.BadRequest(string.Join(", ", removed.Errors.Select(e => e.Description)));
                var added = await _userManager.AddPasswordAsync(user, request.NewPassword);
                if (!added.Succeeded)
                    return Results.BadRequest(string.Join(", ", added.Errors.Select(e => e.Description)));
            }
            else
            {
                if (string.IsNullOrEmpty(request.CurrentPassword))
                    return Results.BadRequest("Current password is required to change your password.");
                var changed = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
                if (!changed.Succeeded)
                    return Results.BadRequest(string.Join(", ", changed.Errors.Select(e => e.Description)));
            }
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return Results.BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        if (string.IsNullOrEmpty(user.Email))
        {
            return Results.Problem("User email cannot be empty.", statusCode: 500);
        }

        var response = new UpdateUserResponse(
            user.Id,
            user.Email ?? string.Empty,
            user.UserName ?? string.Empty,
            user.FirstName ?? string.Empty,
            user.LastName ?? string.Empty,
            user.IsActive
        );

        return Results.Ok(response);
    }
}
