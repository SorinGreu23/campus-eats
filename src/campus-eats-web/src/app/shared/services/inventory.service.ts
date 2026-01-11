import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { InventoryItem, RestockRequest } from '../models/inventory.model';
import { catchError, tap } from 'rxjs/operators';
import { of } from 'rxjs';

const API_BASE_URL = 'http://localhost:5001/api';

@Injectable({
  providedIn: 'root'
})
export class InventoryService {
  private http = inject(HttpClient);
  private apiUrl = `${API_BASE_URL}/inventory`;

  private itemsSignal = signal<InventoryItem[]>([]);
  private loadingSignal = signal(false);
  private errorSignal = signal<string | null>(null);

  items = this.itemsSignal.asReadonly();
  loading = this.loadingSignal.asReadonly();
  error = this.errorSignal.asReadonly();

  loadInventoryItems(forceRefresh = false): void {
    if (this.loadingSignal()) return;
    if (!forceRefresh && this.itemsSignal().length > 0) return;

    this.loadingSignal.set(true);
    this.errorSignal.set(null);

    this.http.get<InventoryItem[]>(this.apiUrl)
      .pipe(
        tap(items => {
          this.itemsSignal.set(items);
          this.loadingSignal.set(false);
        }),
        catchError(error => {
          console.error('Failed to load inventory items:', error);
          this.errorSignal.set('Failed to load inventory items');
          this.loadingSignal.set(false);
          return of([]);
        })
      )
      .subscribe();
  }

  restockItem(itemId: string, request: RestockRequest): void {
    this.http.post(`${this.apiUrl}/${itemId}/restock`, request)
      .pipe(
        tap(() => {
          // Reload inventory after restock
          this.loadInventoryItems(true);
        }),
        catchError(error => {
          console.error('Failed to restock item:', error);
          this.errorSignal.set('Failed to restock item');
          return of(null);
        })
      )
      .subscribe();
  }

  useItem(itemId: string, quantity: number, reason?: string): void {
    this.http.post(`${this.apiUrl}/${itemId}/use`, { quantity, reason })
      .pipe(
        tap(() => {
          // Reload inventory after use
          this.loadInventoryItems(true);
        }),
        catchError(error => {
          console.error('Failed to use item:', error);
          this.errorSignal.set('Failed to use item');
          return of(null);
        })
      )
      .subscribe();
  }
}
