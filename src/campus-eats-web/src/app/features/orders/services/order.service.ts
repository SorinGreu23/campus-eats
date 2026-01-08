import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { DeliveryMethod, Order, OrderItem, OrderStatus, PaymentStatus } from '../models/order.model';
import { AuthStateService } from '../../../shared/services/auth-state.service';

type ApiOrderItem = {
  id: string;
  menuItemId?: string;
  menuItem?: {
    id: string;
    name: string;
    price: number;
    description?: string;
    imageUrl?: string;
  };
  quantity: number;
  unitPrice: number;
  subtotal: number;
  specialInstructions?: string;
};

type ApiOrder = {
  id: string;
  orderNumber?: string;
  status?: string;
  orderType?: string;
  subtotal: number;
  tax: number;
  discount: number;
  total: number;
  deliveryInstructions?: string;
  pickupTime?: string;
  completedAt?: string;
  cancelledAt?: string;
  cancellationReason?: string;
  items: ApiOrderItem[];
};

// TODO: move to environment configuration when available
const API_BASE_URL = 'http://localhost:5001/api';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private http = inject(HttpClient);
  private authState = inject(AuthStateService);

  private orders = signal<Order[]>([]);
  private kitchenOrders = signal<Order[]>([]);
  loading = signal(false);
  kitchenLoading = signal(false);
  error = signal<string | null>(null);
  kitchenError = signal<string | null>(null);

  allOrders = this.orders.asReadonly();

  activeOrders = computed(() =>
    this.orders().filter(order =>
      order.status !== OrderStatus.Completed &&
      order.status !== OrderStatus.Cancelled
    )
  );

  completedOrders = computed(() =>
    this.orders().filter(order =>
      order.status === OrderStatus.Completed ||
      order.status === OrderStatus.Cancelled
    )
  );

  pendingKitchenOrders = this.kitchenOrders.asReadonly();

  loadOrders(force = false): void {
    if (this.loading()) return;
    if (!force && this.orders().length > 0) return;

    const token = this.authState.token();
    if (!token) {
      this.error.set('Please log in to view your orders.');
      this.orders.set([]);
      return;
    }

    const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });

    this.loading.set(true);
    this.error.set(null);

    this.http.get<ApiOrder[]>(`${API_BASE_URL}/orders/user/me`, { headers }).subscribe({
      next: (response) => {
        const mapped = response.map(this.mapOrderFromApi);
        this.orders.set(mapped);
        this.loading.set(false);
      },
      error: (err) => {
        const message = err?.error?.error || 'Unable to load orders';
        this.error.set(message);
        this.loading.set(false);
      }
    });
  }

  loadPendingOrders(force = false): void {
    if (this.kitchenLoading()) return;
    if (!force && this.kitchenOrders().length > 0) return;

    const token = this.authState.token();
    const headers = token ? new HttpHeaders({ Authorization: `Bearer ${token}` }) : undefined;

    this.kitchenLoading.set(true);
    this.kitchenError.set(null);

    this.http.get<ApiOrder[]>(`${API_BASE_URL}/orders/pending`, headers ? { headers } : undefined).subscribe({
      next: (response) => {
        const mapped = response.map(this.mapOrderFromApi);
        this.kitchenOrders.set(mapped);
        this.kitchenLoading.set(false);
      },
      error: (err) => {
        const message = err?.error?.error || 'Unable to load pending orders';
        this.kitchenError.set(message);
        this.kitchenLoading.set(false);
      }
    });
  }

  completeOrder(orderId: string): void {
    const token = this.authState.token();
    if (!token) {
      this.kitchenError.set('Please log in to complete orders.');
      return;
    }

    const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });
    this.http.patch(`${API_BASE_URL}/orders/${orderId}/complete`, {}, { headers }).subscribe({
      next: () => {
        this.kitchenOrders.update(list => list.filter(o => o.id !== orderId));
      },
      error: (err) => {
        const message = err?.error?.error || 'Unable to complete order';
        this.kitchenError.set(message);
      }
    });
  }

  getOrderById(id: string): Order | undefined {
    return this.orders().find(order => order.id === id);
  }

  cancelOrder(orderId: string, reason?: string): void {
    // Backend endpoint currently does not accept orderId correctly; perform optimistic local update.
    this.orders.update(list =>
      list.map(order =>
        order.id === orderId
          ? { ...order, status: OrderStatus.Cancelled, cancellationReason: reason, cancelledAt: new Date() }
          : order
      )
    );
  }

  reorder(order: Order): void {
    // Simple local clone to support UI behavior
    const newOrder: Order = {
      ...order,
      id: crypto.randomUUID(),
      orderNumber: `ORD-${new Date().getFullYear()}-${String(this.orders().length + 1).padStart(3, '0')}`,
      status: OrderStatus.Pending,
      paymentStatus: PaymentStatus.Pending,
      placedAt: new Date(),
      completedAt: undefined,
      cancelledAt: undefined
    };
    this.orders.update(list => [newOrder, ...list]);
  }

  private mapOrderFromApi = (api: ApiOrder): Order => ({
    id: api.id,
    orderNumber: api.orderNumber ?? api.id,
    userId: undefined,
    status: (api.status as OrderStatus) ?? OrderStatus.Pending,
    orderType: (api.orderType as DeliveryMethod) ?? DeliveryMethod.Pickup,
    paymentStatus: PaymentStatus.Paid,
    subtotal: Number(api.subtotal),
    tax: Number(api.tax),
    discount: Number(api.discount ?? 0),
    total: Number(api.total),
    deliveryInstructions: api.deliveryInstructions,
    pickupTime: api.pickupTime ? new Date(api.pickupTime) : undefined,
    estimatedDeliveryTime: api.pickupTime ? new Date(api.pickupTime) : undefined,
    completedAt: api.completedAt ? new Date(api.completedAt) : undefined,
    cancelledAt: api.cancelledAt ? new Date(api.cancelledAt) : undefined,
    cancellationReason: api.cancellationReason,
    items: (api.items || []).map(this.mapOrderItemFromApi),
  });

  private mapOrderItemFromApi = (api: ApiOrderItem): OrderItem => ({
    id: api.id,
    orderId: undefined,
    menuItemId: api.menuItemId,
    menuItemName: api.menuItem?.name ?? 'Menu item',
    menuItemImage: api.menuItem?.imageUrl,
    menuItemDescription: api.menuItem?.description,
    quantity: api.quantity,
    unitPrice: Number(api.unitPrice),
    subtotal: Number(api.subtotal),
    specialInstructions: api.specialInstructions
  });

}
