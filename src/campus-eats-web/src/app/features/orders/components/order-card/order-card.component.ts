import { Component, input, output, computed, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Order, OrderStatus } from '../../models/order.model';

@Component({
  selector: 'app-order-card',
  imports: [CommonModule],
  templateUrl: './order-card.component.html',
  styleUrl: './order-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OrderCardComponent {
  order = input.required<Order>();
  cancelOrder = output<string>();
  reorderOrder = output<Order>();
  viewDetails = output<Order>();

  OrderStatus = OrderStatus;

  statusConfig = computed(() => {
    const status = this.order().status;
    const configs = {
      [OrderStatus.Pending]: { color: 'warning', icon: 'pi-clock', label: 'Pending' },
      [OrderStatus.Confirmed]: { color: 'info', icon: 'pi-check', label: 'Confirmed' },
      [OrderStatus.Preparing]: { color: 'warning', icon: 'pi-spin pi-spinner', label: 'Preparing' },
      [OrderStatus.Ready]: { color: 'success', icon: 'pi-check-circle', label: 'Ready' },
      [OrderStatus.InDelivery]: { color: 'info', icon: 'pi-truck', label: 'On the Way' },
      [OrderStatus.Completed]: { color: 'success', icon: 'pi-check', label: 'Completed' },
      [OrderStatus.Cancelled]: { color: 'danger', icon: 'pi-times-circle', label: 'Cancelled' }
    };
    return configs[status];
  });

  timeRemaining = computed(() => {
    const eta = this.order().estimatedDeliveryTime;
    if (!eta) return null;
    
    const now = new Date();
    const diff = eta.getTime() - now.getTime();
    const minutes = Math.floor(diff / 60000);
    
    if (minutes <= 0) return 'Ready now';
    if (minutes < 60) return `${minutes} min`;
    const hours = Math.floor(minutes / 60);
    const remainingMins = minutes % 60;
    return `${hours}h ${remainingMins}m`;
  });

  formattedDate = computed(() => {
    const date = this.order().placedAt;
    const now = new Date();
    const diffDays = Math.floor((now.getTime() - date.getTime()) / (1000 * 60 * 60 * 24));
    
    if (diffDays === 0) {
      return `Today at ${date.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })}`;
    } else if (diffDays === 1) {
      return 'Yesterday';
    } else if (diffDays < 7) {
      return `${diffDays} days ago`;
    } else {
      return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
    }
  });

  canCancel = computed(() => {
    const status = this.order().status;
    return status === OrderStatus.Pending || status === OrderStatus.Confirmed;
  });

  onCancel(): void {
    if (this.canCancel()) {
      this.cancelOrder.emit(this.order().id);
    }
  }

  onReorder(): void {
    this.reorderOrder.emit(this.order());
  }

  onViewDetails(): void {
    this.viewDetails.emit(this.order());
  }
}
