using System;

namespace CampusEats.Api.Data.Entities;

public class OrderItem : BaseEntity
{
    public Guid? OrderId { get; set; }
    public Guid? MenuItemId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
    public string? SpecialInstructions { get; set; }

    public Order? Order { get; set; }
    public MenuItem? MenuItem { get; set; }
}
