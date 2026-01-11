import { Component, inject, OnInit, signal, computed, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OrderService } from '../../services/order.service';
import { OrderCardComponent } from '../order-card/order-card.component';
import { OrderDetailComponent } from '../order-detail/order-detail.component';
import { Order, OrderStatus } from '../../models/order.model';

@Component({
  selector: 'app-order-list',
  imports: [CommonModule, FormsModule, OrderCardComponent, OrderDetailComponent],
  templateUrl: './order-list.component.html',
  styleUrl: './order-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OrderListComponent implements OnInit {
  private orderService = inject(OrderService);

  activeOrders = this.orderService.activeOrders;
  completedOrders = this.orderService.completedOrders;
  loading = this.orderService.loading;
  error = this.orderService.error;
  showCompleted = signal(false);
  
  // Filter signals
  searchQuery = signal('');

  // Detail modal
  showDetailModal = signal(false);
  selectedOrder = signal<Order | null>(null);
  
  OrderStatus = OrderStatus;

  // Filtered active orders
  filteredActiveOrders = computed(() => {
    let orders = this.activeOrders();
    
    // Filter by search query
    const query = this.searchQuery().toLowerCase();
    if (query) {
      orders = orders.filter(order => 
        (order.orderNumber?.toLowerCase().includes(query) ?? false) ||
        order.items.some(item => item.menuItemName.toLowerCase().includes(query))
      );
    }
    
    // Sort by creation time, newest first
    return [...orders].sort((a, b) => {
      const dateA = a.placedAt?.getTime() ?? 0;
      const dateB = b.placedAt?.getTime() ?? 0;
      return dateB - dateA;
    });
  });

  // Sorted completed orders
  sortedCompletedOrders = computed(() => {
    return [...this.completedOrders()].sort((a, b) => {
      const dateA = a.placedAt?.getTime() ?? 0;
      const dateB = b.placedAt?.getTime() ?? 0;
      return dateB - dateA;
    });
  });

  ngOnInit(): void {
    this.orderService.loadOrders();
  }

  toggleView(): void {
    this.showCompleted.update(value => !value);
  }

  handleCancelOrder(orderId: string): void {
    this.orderService.cancelOrder(orderId);
  }

  handleReorder(order: any): void {
    this.orderService.reorder(order);
  }

  clearFilters(): void {
    this.searchQuery.set('');
  }

  handleViewDetails(order: Order): void {
    this.selectedOrder.set(order);
    // Use setTimeout to ensure the order is set before showing modal
    setTimeout(() => {
      this.showDetailModal.set(true);
    }, 0);
  }

  handleCloseDetail(): void {
    this.showDetailModal.set(false);
    // Clear the order after modal is closed
    setTimeout(() => {
      this.selectedOrder.set(null);
    }, 300);
  }
}
