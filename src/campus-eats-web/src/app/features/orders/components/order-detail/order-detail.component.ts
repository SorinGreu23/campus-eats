import { Component, input, output, computed, viewChild, ElementRef, effect, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DialogModule } from 'primeng/dialog';
import { Order, OrderStatus, PaymentStatus, DeliveryMethod } from '../../models/order.model';

@Component({
  selector: 'app-order-detail',
  imports: [CommonModule, DialogModule],
  templateUrl: './order-detail.component.html',
  styleUrl: './order-detail.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OrderDetailComponent {
  visible = input.required<boolean>();
  order = input<Order | null>(null);
  closeModal = output<void>();
  cancelOrder = output<string>();
  reorderOrder = output<Order>();

  scrollContent = viewChild<ElementRef>('scrollContent');
  
  // Writable signal for two-way binding with dialog
  isVisible = signal(false);
  
  // Track if we're closing to avoid effect re-opening
  private isClosing = false;

  constructor() {
    // Sync visible input with isVisible signal (only when opening)
    effect(() => {
      const visible = this.visible();
      if (visible && !this.isClosing) {
        this.isVisible.set(true);
      }
    }, { allowSignalWrites: true });
    
    effect(() => {
      if (this.visible() && this.scrollContent()) {
        this.resetScroll();
      }
    });
  }

  private resetScroll(): void {
    const element = this.scrollContent()?.nativeElement as HTMLElement | undefined;
    if (!element) return;
    // Multiple attempts to override any late layout shifts or focus jumps
    element.scrollTop = 0;
    requestAnimationFrame(() => {
      element.scrollTop = 0;
      setTimeout(() => {
        element.scrollTop = 0;
      }, 50);
      setTimeout(() => {
        element.scrollTop = 0;
        element.focus({ preventScroll: true });
      }, 150);
    });
  }

  handleDialogShow(): void {
    this.resetScroll();
  }

  OrderStatus = OrderStatus;
  PaymentStatus = PaymentStatus;
  DeliveryMethod = DeliveryMethod;

  statusConfig = computed(() => {
    const status = this.order()?.status as OrderStatus | undefined;
    if (!status) return { color: 'info', icon: 'pi-info-circle', label: 'Unknown' };

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

  paymentStatusConfig = computed(() => {
    const status = this.order()?.paymentStatus;
    if (!status) return { color: 'info', label: 'N/A' };

    const configs = {
      [PaymentStatus.Pending]: { color: 'warning', label: 'Pending' },
      [PaymentStatus.Paid]: { color: 'success', label: 'Paid' },
      [PaymentStatus.Failed]: { color: 'danger', label: 'Failed' },
      [PaymentStatus.Refunded]: { color: 'info', label: 'Refunded' }
    };
    return configs[status];
  });

  canCancel = computed(() => {
    const status = this.order()?.status as OrderStatus | undefined;
    return status === OrderStatus.Pending || status === OrderStatus.Confirmed;
  });

  onDialogHide(): void {
    // Mark as closing to prevent effect from re-opening
    this.isClosing = true;
    this.isVisible.set(false);
    // Notify parent to sync its state
    this.closeModal.emit();
    // Reset closing flag after parent has time to update
    setTimeout(() => {
      this.isClosing = false;
    }, 100);
  }

  onClose(): void {
    this.isClosing = true;
    this.isVisible.set(false);
    this.closeModal.emit();
    setTimeout(() => {
      this.isClosing = false;
    }, 100);
  }

  onCancel(): void {
    const order = this.order();
    if (order && this.canCancel()) {
      this.cancelOrder.emit(order.id);
      this.isVisible.set(false);
    }
  }

  onReorder(): void {
    const order = this.order();
    if (order) {
      this.reorderOrder.emit(order);
      this.isVisible.set(false);
    }
  }
}
