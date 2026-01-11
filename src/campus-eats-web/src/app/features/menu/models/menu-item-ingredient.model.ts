export interface MenuItemIngredient {
  menuItemId: string;
  inventoryItemId: string;
  inventoryItem: {
    id: string;
    name: string;
    unit: string;
    currentQuantity: number;
    minimumQuantity: number;
  };
  quantityRequired: number;
}

export interface AddIngredientRequest {
  inventoryItemId: string;
  quantityRequired: number;
}

export interface UpdateIngredientRequest {
  quantityRequired: number;
}
