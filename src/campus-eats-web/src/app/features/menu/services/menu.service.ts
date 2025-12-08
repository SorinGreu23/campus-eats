import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MenuItem, Category } from '../models/menu-item.model';
import { AuthStateService } from '../../../shared/services/auth-state.service';

type ApiAllergenDto = {
  id: string;
  name: string;
  description?: string;
  icon?: string;
};

type ApiDietaryRestrictionDto = {
  id: string;
  name: string;
  description?: string;
  icon?: string;
};

type ApiMenuItemDto = {
  id: string;
  name: string;
  description?: string;
  price: number;
  categoryName?: string;
  imageUrl?: string;
  preparationTimeMinutes?: number;
  isAvailable: boolean;
  calories?: number;
  createdAt?: string;
  updatedAt?: string;
  allergens?: ApiAllergenDto[];
  dietaryRestrictions?: ApiDietaryRestrictionDto[];
};

type ApiUpsertMenuItemDto = {
  name: string;
  description?: string;
  price: number;
  categoryId?: string | null;
  imageUrl?: string | null;
  preparationTimeMinutes?: number | null;
  isAvailable: boolean;
  calories?: number | null;
  allergenIds?: string[];
  dietaryRestrictionIds?: string[];
};

// TODO: move to environment configuration when available
const API_BASE_URL = 'http://localhost:5001/api';

@Injectable({
  providedIn: 'root'
})
export class MenuService {
  private http = inject(HttpClient);
  private authState = inject(AuthStateService);

  menuItems = signal<MenuItem[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  categories = computed<Category[]>(() => {
    const seen = new Set<string>();
    return this.menuItems()
      .filter(item => !!item.categoryName)
      .map(item => item.categoryName as string)
      .filter(name => {
        if (seen.has(name)) return false;
        seen.add(name);
        return true;
      })
      .map(name => ({ id: name, name, isActive: true }));
  });

  loadMenuItems(force = false): void {
    if (this.loading()) return;
    if (!force && this.menuItems().length > 0) return;

    this.loading.set(true);
    this.error.set(null);

    this.http.get<ApiMenuItemDto[]>(`${API_BASE_URL}/menuitems`).subscribe({
      next: (response) => {
        const mapped = response.map(this.mapMenuItemFromApi);
        this.menuItems.set(mapped);
        this.loading.set(false);
      },
      error: (err) => {
        const message = err?.error?.title || 'Unable to load menu items';
        this.error.set(message);
        this.loading.set(false);
      }
    });
  }

  getMenuItemById(id: string): MenuItem | undefined {
    return this.menuItems().find(item => item.id === id);
  }

  createMenuItem(payload: ApiUpsertMenuItemDto): void {
    const token = this.authState.token();
    if (!token) {
      this.error.set('Please log in to manage menu items.');
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.http.post(`${API_BASE_URL}/menuitems`, payload, {
      headers: { Authorization: `Bearer ${token}` }
    }).subscribe({
      next: () => {
        this.loadMenuItems(true);
        this.loading.set(false);
      },
      error: (err) => {
        const message = err?.error?.error || err?.error?.title || 'Unable to create menu item';
        this.error.set(message);
        this.loading.set(false);
      }
    });
  }

  updateMenuItem(id: string, payload: ApiUpsertMenuItemDto): void {
    const token = this.authState.token();
    if (!token) {
      this.error.set('Please log in to manage menu items.');
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.http.put(`${API_BASE_URL}/menuitems/${id}`, payload, {
      headers: { Authorization: `Bearer ${token}` }
    }).subscribe({
      next: () => {
        this.loadMenuItems(true);
        this.loading.set(false);
      },
      error: (err) => {
        const message = err?.error?.error || err?.error?.title || 'Unable to update menu item';
        this.error.set(message);
        this.loading.set(false);
      }
    });
  }

  deleteMenuItem(id: string): void {
    const token = this.authState.token();
    if (!token) {
      this.error.set('Please log in to manage menu items.');
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.http.delete(`${API_BASE_URL}/menuitems/${id}`, {
      headers: { Authorization: `Bearer ${token}` }
    }).subscribe({
      next: () => {
        this.menuItems.update(items => items.filter(item => item.id !== id));
        this.loading.set(false);
      },
      error: (err) => {
        const message = err?.error?.error || err?.error?.title || 'Unable to delete menu item';
        this.error.set(message);
        this.loading.set(false);
      }
    });
  }

  private mapMenuItemFromApi = (apiItem: ApiMenuItemDto): MenuItem => ({
    id: apiItem.id,
    name: apiItem.name,
    description: apiItem.description ?? '',
    price: Number(apiItem.price),
    categoryName: apiItem.categoryName,
    imageUrl: apiItem.imageUrl,
    preparationTimeMinutes: apiItem.preparationTimeMinutes ?? undefined,
    isAvailable: apiItem.isAvailable,
    calories: apiItem.calories ?? undefined,
    createdAt: apiItem.createdAt,
    updatedAt: apiItem.updatedAt,
    allergens: (apiItem.allergens || [])
      .filter(a => !!a.name)
      .map(a => ({ name: a.name, icon: a.icon, description: a.description })),
    dietaryTags: (apiItem.dietaryRestrictions || []).map(d => d.name).filter(Boolean)
  });
}
