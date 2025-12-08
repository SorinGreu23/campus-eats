export enum OrderStatus {
  Pending = 'Pending',
  Confirmed = 'Confirmed',
  Preparing = 'Preparing',
  Ready = 'Ready',
  InDelivery = 'InDelivery',
  Completed = 'Completed',
  Cancelled = 'Cancelled'
}

export enum PaymentStatus {
  Pending = 'Pending',
  Paid = 'Paid',
  Failed = 'Failed',
  Refunded = 'Refunded'
}

export enum DeliveryMethod {
  Pickup = 'Pickup',
  Delivery = 'Delivery'
}

export interface OrderItem {
  id: string;
  orderId?: string;
  menuItemId?: string;
  menuItemName: string;
  menuItemImage?: string;
  menuItemDescription?: string;
  quantity: number;
  unitPrice: number;
  subtotal: number;
  specialInstructions?: string;
}

export interface Order {
  id: string;
  orderNumber?: string;
  userId?: string;
  status?: OrderStatus | string;
  orderType?: DeliveryMethod | string;
  paymentStatus?: PaymentStatus;
  subtotal: number;
  tax: number;
  discount: number;
  total: number;
  deliveryInstructions?: string;
  pickupTime?: Date;
  estimatedDeliveryTime?: Date;
  placedAt?: Date;
  confirmedAt?: Date;
  completedAt?: Date;
  cancelledAt?: Date;
  cancellationReason?: string;
  items: OrderItem[];
  specialInstructions?: string;
}
