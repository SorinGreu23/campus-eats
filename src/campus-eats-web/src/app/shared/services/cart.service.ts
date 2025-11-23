import { Injectable, signal, computed } from '@angular/core';
import { CartItem } from '../models/cart-item.model';
import { MenuItem } from '../../features/menu/models/menu-item.model';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private cartItems = signal<CartItem[]>([]);

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

  tax = computed(() => this.subtotal() * 0.08); // 8% tax
  
  total = computed(() => this.subtotal() + this.tax());

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
        return newItems;
      });
    } else {
      // Add new item
      this.cartItems.update(items => [...items, { menuItem, quantity }]);
    }
  }

  updateQuantity(menuItemId: string, quantity: number): void {
    if (quantity <= 0) {
      this.removeItem(menuItemId);
      return;
    }

    this.cartItems.update(items =>
      items.map(item =>
        item.menuItem.id === menuItemId
          ? { ...item, quantity }
          : item
      )
    );
  }

  removeItem(menuItemId: string): void {
    this.cartItems.update(items =>
      items.filter(item => item.menuItem.id !== menuItemId)
    );
  }

  clearCart(): void {
    this.cartItems.set([]);
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
