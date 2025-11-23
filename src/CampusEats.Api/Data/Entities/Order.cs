using System;
using System.Collections.Generic;

namespace CampusEats.Api.Data.Entities;

public class Order
{
    public Guid Id { get; set; }
    public string? OrderNumber { get; set; }
    public string? UserId { get; set; }
    public string? Status { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public string? SpecialInstructions { get; set; }
    public DateTimeOffset? PickupTime { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    public ApplicationUser? User { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
