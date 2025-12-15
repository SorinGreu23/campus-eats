export interface MenuItem {
  id: string;
  name: string;
  price: number;
  categoryName?: string;
  imageUrl?: string;
  preparationTimeMinutes?: number;
  isAvailable: boolean;
  calories?: number;
  createdAt?: string;
  updatedAt?: string;
  description: string;
  allergens?: MenuAllergen[];
  dietaryTags?: string[];
}

export interface MenuAllergen {
  name: string;
  icon?: string;
  description?: string;
}

export interface Category {
  id: string;
  name: string;
  isActive: boolean;
}
