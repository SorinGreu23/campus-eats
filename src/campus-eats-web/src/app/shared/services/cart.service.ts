import { Injectable, signal, computed } from '@angular/core';
import { CartItem } from '../models/cart-item.model';
import { MenuItem } from '../../features/menu/models/menu-item.model';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private readonly STORAGE_KEY = 'campus_eats_cart';
  private cartItems = signal<CartItem[]>(this.loadFromStorage());

  // Computed signals for derived state
  items = this.cartItems.asReadonly();
  
  itemCount = computed(() => 
    this.cartItems().reduce((total, item) => total + item.quantity, 0)
  );
  
  subtotal = computed(() => 
    this.cartItems().reduce((total, item) => 
      total + (item.menuItem.price * item.quantity), 0
    )
  );

  tax = computed(() => Math.round(this.subtotal() * 0.21 * 100) / 100); // 21% tax to match backend
  
  total = computed(() => Math.round((this.subtotal() + this.tax()) * 100) / 100);

  addItem(menuItem: MenuItem, quantity: number = 1): void {
    const currentItems = this.cartItems();
    const existingItemIndex = currentItems.findIndex(
      item => item.menuItem.id === menuItem.id
    );

    if (existingItemIndex >= 0) {
      // Update existing item quantity
      this.cartItems.update(items => {
        const newItems = [...items];
        newItems[existingItemIndex] = {
          ...newItems[existingItemIndex],
          quantity: newItems[existingItemIndex].quantity + quantity
        };
        this.saveToStorage(newItems);
        return newItems;
      });
    } else {
      // Add new item
      this.cartItems.update(items => {
        const newItems = [...items, { menuItem, quantity }];
        this.saveToStorage(newItems);
        return newItems;
      });
    }
  }

  updateQuantity(menuItemId: string, quantity: number): void {
    if (quantity <= 0) {
      this.removeItem(menuItemId);
      return;
    }

    this.cartItems.update(items => {
      const newItems = items.map(item =>
        item.menuItem.id === menuItemId
          ? { ...item, quantity }
          : item
      );
      this.saveToStorage(newItems);
      return newItems;
    });
  }

  removeItem(menuItemId: string): void {
    this.cartItems.update(items => {
      const newItems = items.filter(item => item.menuItem.id !== menuItemId);
      this.saveToStorage(newItems);
      return newItems;
    });
  }

  clearCart(): void {
    this.cartItems.set([]);
    this.saveToStorage([]);
  }

  private loadFromStorage(): CartItem[] {
    try {
      const stored = localStorage.getItem(this.STORAGE_KEY);
      return stored ? JSON.parse(stored) : [];
    } catch {
      return [];
    }
  }

  private saveToStorage(items: CartItem[]): void {
    try {
      localStorage.setItem(this.STORAGE_KEY, JSON.stringify(items));
    } catch {
      // Ignore storage errors
    }
  }

  // Initialize with some mock data for testing
  initializeMockData(): void {
    const mockItems: MenuItem[] = [
      {
        id: '1',
        name: 'Pepperoni Pizza',
        description: 'Classic pepperoni with mozzarella cheese',
        price: 12.99,
        imageUrl: 'https://via.placeholder.com/100x100?text=Pizza',
        isAvailable: true,
        calories: 280
      },
      {
        id: '2',
        name: 'Cheeseburger',
        description: 'Juicy beef patty with cheese',
        price: 9.99,
        imageUrl: 'https://via.placeholder.com/100x100?text=Burger',
        isAvailable: true,
        calories: 450
      }
    ];

    this.cartItems.set([
      { menuItem: mockItems[0], quantity: 2 },
      { menuItem: mockItems[1], quantity: 1 }
    ]);
  }
}
