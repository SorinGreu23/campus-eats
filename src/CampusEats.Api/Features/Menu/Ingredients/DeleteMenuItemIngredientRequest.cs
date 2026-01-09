using MediatR;

namespace CampusEats.Api.Features.Menu.Ingredients;

public record DeleteMenuItemIngredientRequest(
    Guid MenuItemId,
    Guid InventoryItemId
) : IRequest<IResult>;
