import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { MenuService } from '../../../menu/services/menu.service';
import { OrderService } from '../../../orders/services/order.service';
import { MenuItem } from '../../../menu/models/menu-item.model';
import { MenuItemFormModalComponent } from '../menu-item-form-modal/menu-item-form-modal.component';
import { InventoryListComponent } from '../inventory-list/inventory-list.component';
import { IngredientManagerComponent } from '../ingredient-manager/ingredient-manager.component';

@Component({
  selector: 'app-kitchen-dashboard',
  standalone: true,
  imports: [CommonModule, MenuItemFormModalComponent, InventoryListComponent, IngredientManagerComponent],
  templateUrl: './kitchen-dashboard.component.html',
  styleUrl: './kitchen-dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class KitchenDashboardComponent implements OnInit {
  private menuService = inject(MenuService);
  private orderService = inject(OrderService);

  menuItems = this.menuService.menuItems;
  menuLoading = this.menuService.loading;
  menuError = this.menuService.error;

  pendingOrders = this.orderService.pendingKitchenOrders;
  ordersLoading = this.orderService.kitchenLoading;
  ordersError = this.orderService.kitchenError;

  categories = this.menuService.categories;

  showFormModal = signal(false);
  editingItem = signal<MenuItem | null>(null);
  
  activeTab = signal<'orders' | 'menu' | 'inventory' | 'ingredients'>('orders');

  ngOnInit(): void {
    this.menuService.loadMenuItems();
    this.orderService.loadPendingOrders(true);
  }

  openCreateModal(): void {
    this.editingItem.set(null);
    this.showFormModal.set(true);
  }

  openEditModal(item: MenuItem): void {
    this.editingItem.set(item);
    this.showFormModal.set(true);
  }

  closeFormModal(): void {
    this.showFormModal.set(false);
    this.editingItem.set(null);
  }

  deleteItem(id: string): void {
    const confirmed = confirm('Delete this menu item?');
    if (!confirmed) return;
    this.menuService.deleteMenuItem(id);
  }

  markOrderComplete(orderId: string): void {
    this.orderService.completeOrder(orderId);
  }

  refresh(): void {
    this.menuService.loadMenuItems(true);
    this.orderService.loadPendingOrders(true);
  }
  
  setActiveTab(tab: 'orders' | 'menu' | 'inventory' | 'ingredients'): void {
    this.activeTab.set(tab);
  }
}
