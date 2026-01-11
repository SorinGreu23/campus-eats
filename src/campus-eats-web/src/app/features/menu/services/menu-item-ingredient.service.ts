import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthStateService } from '../../../shared/services/auth-state.service';
import { MenuItemIngredient, AddIngredientRequest } from '../models/menu-item-ingredient.model';

const API_BASE_URL = 'http://localhost:5001/api';

@Injectable({
  providedIn: 'root'
})
export class MenuItemIngredientService {
  private http = inject(HttpClient);
  private authState = inject(AuthStateService);

  ingredients = signal<MenuItemIngredient[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  loadIngredients(menuItemId: string): void {
    const token = this.authState.token();
    const headers = token ? new HttpHeaders({ Authorization: `Bearer ${token}` }) : undefined;

    this.loading.set(true);
    this.error.set(null);

    this.http.get<MenuItemIngredient[]>(`${API_BASE_URL}/menuitems/${menuItemId}/ingredients`, headers ? { headers } : undefined)
      .subscribe({
        next: (response) => {
          this.ingredients.set(response);
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set(err?.error?.error || 'Failed to load ingredients');
          this.loading.set(false);
        }
      });
  }

  addIngredient(menuItemId: string, request: AddIngredientRequest): Promise<void> {
    const token = this.authState.token();
    if (!token) {
      return Promise.reject('Authentication required');
    }

    const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });

    return new Promise((resolve, reject) => {
      this.http.post(`${API_BASE_URL}/menuitems/${menuItemId}/ingredients`, request, { headers })
        .subscribe({
          next: () => {
            this.loadIngredients(menuItemId);
            resolve();
          },
          error: (err) => reject(err?.error?.error || 'Failed to add ingredient')
        });
    });
  }

  updateIngredient(menuItemId: string, inventoryItemId: string, quantityRequired: number): Promise<void> {
    const token = this.authState.token();
    if (!token) {
      return Promise.reject('Authentication required');
    }

    const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });

    return new Promise((resolve, reject) => {
      this.http.put(
        `${API_BASE_URL}/menuitems/${menuItemId}/ingredients/${inventoryItemId}`,
        { quantityRequired },
        { headers }
      ).subscribe({
        next: () => {
          this.loadIngredients(menuItemId);
          resolve();
        },
        error: (err) => reject(err?.error?.error || 'Failed to update ingredient')
      });
    });
  }

  deleteIngredient(menuItemId: string, inventoryItemId: string): Promise<void> {
    const token = this.authState.token();
    if (!token) {
      return Promise.reject('Authentication required');
    }

    const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });

    return new Promise((resolve, reject) => {
      this.http.delete(
        `${API_BASE_URL}/menuitems/${menuItemId}/ingredients/${inventoryItemId}`,
        { headers }
      ).subscribe({
        next: () => {
          this.loadIngredients(menuItemId);
          resolve();
        },
        error: (err) => reject(err?.error?.error || 'Failed to delete ingredient')
      });
    });
  }
}
