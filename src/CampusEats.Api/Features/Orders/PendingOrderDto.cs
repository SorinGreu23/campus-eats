namespace CampusEats.Api.Features.Kitchen;

public class PendingOrderDto
{
    public Guid Id { get; set; }
    public string? OrderNumber { get; set; }
    public string? Status { get; set; }
    public decimal Total { get; set; }
    public string? SpecialInstructions { get; set; }
    public DateTimeOffset? PickupTime { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public IEnumerable<PendingOrderItemDto> Items { get; set; }
}

public class PendingOrderItemDto
{
    public Guid Id { get; set; }
    public string? MenuItemName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
    public string? SpecialInstructions { get; set; }
}

