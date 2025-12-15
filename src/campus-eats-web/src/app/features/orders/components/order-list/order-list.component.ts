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
  selectedStatus = signal<OrderStatus | 'all'>('all');

  // Detail modal
  showDetailModal = signal(false);
  selectedOrder = signal<Order | null>(null);
  
  OrderStatus = OrderStatus;
  statusOptions = [
    { value: 'all', label: 'All Statuses' },
    { value: OrderStatus.Pending, label: 'Pending' },
    { value: OrderStatus.Confirmed, label: 'Confirmed' },
    { value: OrderStatus.Preparing, label: 'Preparing' },
    { value: OrderStatus.Ready, label: 'Ready' },
    { value: OrderStatus.InDelivery, label: 'In Delivery' },
  ];

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
    
    // Filter by status
    const status = this.selectedStatus();
    if (status !== 'all') {
      orders = orders.filter(order => order.status === status);
    }
    
    return orders;
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
    this.selectedStatus.set('all');
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
