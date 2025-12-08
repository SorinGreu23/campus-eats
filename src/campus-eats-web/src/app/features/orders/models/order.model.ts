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
  orderId: string;
  menuItemId: string;
  menuItemName: string;
  menuItemImage?: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  specialInstructions?: string;
}

export interface Order {
  id: string;
  orderNumber: string;
  userId: string;
  status: OrderStatus;
  deliveryMethod: DeliveryMethod;
  deliveryAddress?: string;
  paymentStatus: PaymentStatus;
  subtotal: number;
  taxAmount: number;
  deliveryFee: number;
  totalAmount: number;
  estimatedDeliveryTime?: Date;
  placedAt: Date;
  confirmedAt?: Date;
  completedAt?: Date;
  cancelledAt?: Date;
  items: OrderItem[];
  specialInstructions?: string;
}
