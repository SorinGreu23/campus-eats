using System;
using System.Collections.Generic;

namespace CampusEats.Api.Data.Entities;

public class Order
{
    public Guid Id { get; set; }
    public required string OrderNumber { get; set; }
    public required string UserId { get; set; }
    public required string Status { get; set; }
    public string? OrderType { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public Guid? LoyaltyRewardId { get; set; }
    public string? DeliveryInstructions { get; set; }
    public DateTimeOffset? PickupTime { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public ApplicationUser? User { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
