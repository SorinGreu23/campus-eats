using System;

namespace CampusEats.Api.Data.Entities;

public class InventoryTransaction
{
    public Guid Id { get; set; }
    public Guid InventoryItemId { get; set; }
    public string TransactionType { get; set; } = null!;
    public decimal Quantity { get; set; }
    public string? Reason { get; set; }
    public string PerformedBy { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }

    public InventoryItem? InventoryItem { get; set; }
}
