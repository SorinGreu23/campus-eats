using MediatR;

namespace CampusEats.Api.Features.Menu.Ingredients;

public record GetMenuItemIngredientsRequest(Guid MenuItemId) : IRequest<IResult>;
