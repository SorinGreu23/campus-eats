export interface InventoryItem {
  id: string;
  name: string;
  unit: string;
  currentQuantity: number;
  minimumQuantity: number;
  isLowStock: boolean;
  isOutOfStock: boolean;
  updatedAt: string;
}

export interface InventoryTransaction {
  id: string;
  inventoryItemId: string;
  itemName: string;
  transactionType: 'Restock' | 'Use' | 'Adjustment';
  quantity: number;
  reason?: string;
  performedBy: string;
  createdAt: string;
}

export interface RestockRequest {
  quantity: number;
  reason?: string;
}
