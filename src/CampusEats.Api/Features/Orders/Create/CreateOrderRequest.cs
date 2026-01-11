using System.Collections.Generic;
using MediatR;

namespace CampusEats.Api.Features.Orders.Create;

public class CreateOrderRequest : IRequest<IResult>
{
    public string? UserId { get; set; }
    public string? DeliveryInstructions { get; set; }
    public string? OrderType { get; set; }
    public ICollection<CreateOrderItemRequest>? Items { get; set; }
    public Guid? LoyaltyRewardId { get; set; }
}

public class CreateOrderItemRequest
{
    public Guid? MenuItemId { get; set; }
    public int Quantity { get; set; }
    public string? SpecialInstructions { get; set; }
}
