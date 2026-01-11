using MediatR;

namespace CampusEats.Api.Features.Menu.Ingredients;

public record UpdateMenuItemIngredientRequest(
    Guid MenuItemId,
    Guid InventoryItemId,
    decimal QuantityRequired
) : IRequest<IResult>;
