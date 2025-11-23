using CampusEats.Api.Data;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Menu;

public class UpdateItemHandler : IRequestHandler<UpdateItemCommand, IResult>
{
    private readonly CampusDbContext _context;
    private readonly IValidator<UpdateItemRequest> _validator;

    public UpdateItemHandler(CampusDbContext context, IValidator<UpdateItemRequest> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<IResult> Handle(UpdateItemCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command.Request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );
            
            return Results.BadRequest(new { errors });
        }

        var menuItem = await _context.MenuItems
            .FirstOrDefaultAsync(m => m.Id == command.Id, cancellationToken);

        if (menuItem == null)
        {
            return Results.NotFound($"Menu item with ID '{command.Id}' was not found.");
        }

        menuItem.Name = command.Request.Name;
        menuItem.Description = command.Request.Description;
        menuItem.Price = command.Request.Price;
        menuItem.CategoryId = command.Request.CategoryId;
        menuItem.ImageUrl = command.Request.ImageUrl;
        menuItem.PreparationTimeMinutes = command.Request.PreparationTimeMinutes;
        menuItem.IsAvailable = command.Request.IsAvailable;
        menuItem.Calories = command.Request.Calories;
        menuItem.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        
        return Results.NoContent();
    }
}

