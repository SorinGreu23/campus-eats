﻿using CampusEats.Api.Data.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using CampusEats.Api.Common;

namespace CampusEats.Api.Features.Users.Update;

public class UpdateUserHandler : IRequestHandler<UpdateUserRequest, IResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<UpdateUserRequest> _validator;

    public UpdateUserHandler(UserManager<ApplicationUser> userManager, IValidator<UpdateUserRequest> validator)
    {
        _userManager = userManager;
        _validator = validator;
    }

    public async Task<IResult> Handle(UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
        }

        var user = await _userManager.FindByIdAsync(request.Id);

        if (user == null)
        {
            return Results.BadRequest("User not found");
        }

        if (!string.IsNullOrEmpty(request.FirstName))
            user.FirstName = request.FirstName;

        if (!string.IsNullOrEmpty(request.LastName))
            user.LastName = request.LastName;

        if (request.IsActive.HasValue)
            user.IsActive = request.IsActive.Value;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return Results.BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        var response = new UpdateUserResponse(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName ?? string.Empty,
            user.LastName ?? string.Empty,
            user.IsActive
        );

        return Results.Ok(response);
    }
}
