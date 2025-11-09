using CampusEats.Api.Data;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Features.Menu;

public class UpdateItemHandler : IRequestHandler<UpdateItemRequest, bool>
{
    private readonly CampusDbContext _context;
    private readonly IValidator<UpdateItemRequest> _validator;

    public UpdateItemHandler(CampusDbContext context, IValidator<UpdateItemRequest> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<bool> Handle(UpdateItemRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var menuItem = await _context.MenuItems
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (menuItem == null)
            return false;

        menuItem.Name = request.Name;
        menuItem.Description = request.Description;
        menuItem.Price = request.Price;
        menuItem.CategoryId = request.CategoryId;
        menuItem.ImageUrl = request.ImageUrl;
        menuItem.PreparationTimeMinutes = request.PreparationTimeMinutes;
        menuItem.IsAvailable = request.IsAvailable;
        menuItem.Calories = request.Calories;
        menuItem.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

