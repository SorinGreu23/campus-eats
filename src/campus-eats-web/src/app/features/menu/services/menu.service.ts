import { Injectable, signal } from '@angular/core';
import { MenuItem, Category } from '../models/menu-item.model';

@Injectable({
  providedIn: 'root'
})
export class MenuService {
  // Mock categories data
  private mockCategories: Category[] = [
    {
      id: '1',
      name: 'Burgers',
      displayOrder: 1,
      isActive: true
    },
    {
      id: '2',
      name: 'Pizza',
      displayOrder: 2,
      isActive: true
    },
    {
      id: '3',
      name: 'Salads',
      displayOrder: 3,
      isActive: true
    },
    {
      id: '4',
      name: 'Beverages',
      displayOrder: 4,
      isActive: true
    },
    {
      id: '5',
      name: 'Desserts',
      displayOrder: 5,
      isActive: true
    }
  ];

  // Mock menu items data
  private mockMenuItems: MenuItem[] = [
    {
      id: '1',
      name: 'Classic Burger',
      description: 'Juicy beef patty with lettuce, tomato, onion, pickles, and our special sauce',
      price: 8.99,
      categoryId: '1',
      imageUrl: 'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=400&h=300&fit=crop',
      preparationTimeMinutes: 15,
      isAvailable: true,
      calories: 650
    },
    {
      id: '2',
      name: 'Bacon Cheeseburger',
      description: 'Double beef patty with crispy bacon, melted cheddar, and BBQ sauce',
      price: 11.99,
      categoryId: '1',
      imageUrl: 'https://images.unsplash.com/photo-1550547660-d9450f859349?w=400&h=300&fit=crop',
      preparationTimeMinutes: 18,
      isAvailable: true,
      calories: 850
    },
    {
      id: '3',
      name: 'Veggie Burger',
      description: 'Plant-based patty with avocado, sprouts, and chipotle mayo',
      price: 9.99,
      categoryId: '1',
      imageUrl: 'https://images.unsplash.com/photo-1520072959219-c595dc870360?w=400&h=300&fit=crop',
      preparationTimeMinutes: 12,
      isAvailable: true,
      calories: 450
    },
    {
      id: '4',
      name: 'Margherita Pizza',
      description: 'Fresh mozzarella, tomato sauce, basil, and olive oil',
      price: 12.99,
      categoryId: '2',
      imageUrl: 'https://images.unsplash.com/photo-1574071318508-1cdbab80d002?w=400&h=300&fit=crop',
      preparationTimeMinutes: 20,
      isAvailable: true,
      calories: 720
    },
    {
      id: '5',
      name: 'Pepperoni Pizza',
      description: 'Classic pepperoni with extra cheese and our signature tomato sauce',
      price: 14.99,
      categoryId: '2',
      imageUrl: 'https://images.unsplash.com/photo-1628840042765-356cda07504e?w=400&h=300&fit=crop',
      preparationTimeMinutes: 22,
      isAvailable: true,
      calories: 890
    },
    {
      id: '6',
      name: 'Veggie Supreme Pizza',
      description: 'Bell peppers, mushrooms, onions, olives, and tomatoes',
      price: 13.99,
      categoryId: '2',
      imageUrl: 'https://images.unsplash.com/photo-1571997478779-2adcbbe9ab2f?w=400&h=300&fit=crop',
      preparationTimeMinutes: 20,
      isAvailable: true,
      calories: 680
    },
    {
      id: '7',
      name: 'Caesar Salad',
      description: 'Crisp romaine lettuce, parmesan, croutons, and Caesar dressing',
      price: 7.99,
      categoryId: '3',
      imageUrl: 'https://images.unsplash.com/photo-1546793665-c74683f339c1?w=400&h=300&fit=crop',
      preparationTimeMinutes: 8,
      isAvailable: true,
      calories: 320
    },
    {
      id: '8',
      name: 'Greek Salad',
      description: 'Tomatoes, cucumbers, olives, feta cheese, and olive oil',
      price: 8.99,
      categoryId: '3',
      imageUrl: 'https://images.unsplash.com/photo-1540189549336-e6e99c3679fe?w=400&h=300&fit=crop',
      preparationTimeMinutes: 10,
      isAvailable: true,
      calories: 280
    },
    {
      id: '9',
      name: 'Cobb Salad',
      description: 'Mixed greens, chicken, bacon, egg, avocado, and blue cheese',
      price: 10.99,
      categoryId: '3',
      imageUrl: 'https://images.unsplash.com/photo-1607532941433-304659e8198a?w=400&h=300&fit=crop',
      preparationTimeMinutes: 12,
      isAvailable: true,
      calories: 520
    },
    {
      id: '10',
      name: 'Coca-Cola',
      description: 'Classic refreshing cola beverage',
      price: 2.49,
      categoryId: '4',
      imageUrl: 'https://images.unsplash.com/photo-1554866585-cd94860890b7?w=400&h=300&fit=crop',
      preparationTimeMinutes: 2,
      isAvailable: true,
      calories: 140
    },
    {
      id: '11',
      name: 'Fresh Lemonade',
      description: 'Homemade lemonade with fresh lemons and mint',
      price: 3.99,
      categoryId: '4',
      imageUrl: 'https://images.unsplash.com/photo-1523677011781-c91d1bbe2f9d?w=400&h=300&fit=crop',
      preparationTimeMinutes: 5,
      isAvailable: true,
      calories: 120
    },
    {
      id: '12',
      name: 'Iced Coffee',
      description: 'Cold brew coffee with ice and your choice of milk',
      price: 4.49,
      categoryId: '4',
      imageUrl: 'https://images.unsplash.com/photo-1517487881594-2787fef5ebf7?w=400&h=300&fit=crop',
      preparationTimeMinutes: 3,
      isAvailable: true,
      calories: 80
    },
    {
      id: '13',
      name: 'Chocolate Brownie',
      description: 'Warm chocolate brownie with vanilla ice cream',
      price: 5.99,
      categoryId: '5',
      imageUrl: 'https://images.unsplash.com/photo-1607920591413-4ec007e70023?w=400&h=300&fit=crop',
      preparationTimeMinutes: 8,
      isAvailable: true,
      calories: 480
    },
    {
      id: '14',
      name: 'Cheesecake',
      description: 'New York style cheesecake with berry compote',
      price: 6.99,
      categoryId: '5',
      imageUrl: 'https://images.unsplash.com/photo-1533134486753-c833f0ed4866?w=400&h=300&fit=crop',
      preparationTimeMinutes: 5,
      isAvailable: true,
      calories: 420
    },
    {
      id: '15',
      name: 'Apple Pie',
      description: 'Classic apple pie with cinnamon and a flaky crust',
      price: 5.49,
      categoryId: '5',
      imageUrl: 'https://images.unsplash.com/photo-1535920527002-b35e96722eb9?w=400&h=300&fit=crop',
      preparationTimeMinutes: 6,
      isAvailable: true,
      calories: 350
    }
  ];

  // Signals for reactive state
  categories = signal<Category[]>(this.mockCategories);
  menuItems = signal<MenuItem[]>(this.mockMenuItems);
  selectedCategory = signal<string | null>(null);

  constructor() {}

  getCategories() {
    return this.categories();
  }

  getMenuItems(categoryId?: string) {
    if (categoryId) {
      return this.menuItems().filter(item => item.categoryId === categoryId);
    }
    return this.menuItems();
  }

  getMenuItemById(id: string) {
    return this.menuItems().find(item => item.id === id);
  }

  selectCategory(categoryId: string | null) {
    this.selectedCategory.set(categoryId);
  }

  // TODO: Replace with actual API calls when backend is ready
  // async fetchCategories() {
  //   const response = await fetch('/api/categories');
  //   const data = await response.json();
  //   this.categories.set(data);
  // }

  // async fetchMenuItems() {
  //   const response = await fetch('/api/menu-items');
  //   const data = await response.json();
  //   this.menuItems.set(data);
  // }
}
