export interface MenuItem {
  id: string;
  name: string;
  description?: string;
  price: number;
  categoryId?: string;
  imageUrl?: string;
  preparationTimeMinutes?: number;
  isAvailable: boolean;
  calories?: number;
  category?: Category;
  allergens?: string[];
  dietaryTags?: string[];
}

export interface Category {
  id: string;
  name: string;
  displayOrder?: number;
  isActive: boolean;
}
