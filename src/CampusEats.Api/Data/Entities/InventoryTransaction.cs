using System;

namespace CampusEats.Api.Data.Entities;

public class InventoryTransaction : BaseEntity
{
    public Guid? InventoryItemId { get; set; }
    public string? TransactionType { get; set; }
    public decimal Quantity { get; set; }
    public string? Reason { get; set; }
    public Guid? PerformedBy { get; set; }

    public InventoryItem? InventoryItem { get; set; }
}
