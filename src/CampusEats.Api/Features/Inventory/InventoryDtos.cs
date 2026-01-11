namespace CampusEats.Api.Features.Inventory;

public record InventoryItemDto(
    Guid Id,
    string Name,
    string Unit,
    decimal CurrentQuantity,
    decimal MinimumQuantity,
    bool IsLowStock,
    bool IsOutOfStock,
    DateTimeOffset UpdatedAt
);

public record InventoryTransactionDto(
    Guid Id,
    Guid InventoryItemId,
    string ItemName,
    string TransactionType,
    decimal Quantity,
    string? Reason,
    string PerformedBy,
    DateTimeOffset CreatedAt
);
