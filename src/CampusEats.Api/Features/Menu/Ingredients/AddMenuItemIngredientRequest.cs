using MediatR;

namespace CampusEats.Api.Features.Menu.Ingredients;

public record AddMenuItemIngredientRequest(
    Guid MenuItemId,
    Guid InventoryItemId,
    decimal QuantityRequired
) : IRequest<IResult>;
