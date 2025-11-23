import { Injectable, signal, computed } from '@angular/core';
import { Order, OrderStatus, PaymentStatus, DeliveryMethod } from '../models/order.model';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private orders = signal<Order[]>([]);

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

  getOrderById(id: string): Order | undefined {
    return this.orders().find(order => order.id === id);
  }

  cancelOrder(orderId: string): void {
    this.orders.update(orders => 
      orders.map(order => 
        order.id === orderId 
          ? { ...order, status: OrderStatus.Cancelled }
          : order
      )
    );
  }

  reorder(order: Order): void {
    const newOrder: Order = {
      ...order,
      id: Date.now().toString(),
      orderNumber: `ORD-${new Date().getFullYear()}-${String(this.orders().length + 1).padStart(3, '0')}`,
      status: OrderStatus.Pending,
      paymentStatus: PaymentStatus.Pending,
      placedAt: new Date(),
      confirmedAt: undefined,
      completedAt: undefined,
      estimatedDeliveryTime: new Date(Date.now() + 30 * 60 * 1000)
    };
    
    this.orders.update(orders => [newOrder, ...orders]);
  }

  // Initialize with mock data
  initializeMockData(): void {
    const menuItems = [
      { name: 'Pepperoni Pizza', image: 'https://images.unsplash.com/photo-1628840042765-356cda07504e?w=80', price: 12.99 },
      { name: 'Classic Burger', image: 'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=80', price: 9.99 },
      { name: 'Caesar Salad', image: 'https://images.unsplash.com/photo-1546793665-c74683f339c1?w=80', price: 8.99 },
      { name: 'Chicken Wrap', image: 'https://images.unsplash.com/photo-1626700051175-6818013e1d4f?w=80', price: 7.49 },
      { name: 'French Fries', image: 'https://images.unsplash.com/photo-1573080496219-bb080dd4f877?w=80', price: 3.50 },
      { name: 'Sushi Roll', image: 'https://images.unsplash.com/photo-1579584425555-c3ce17fd4351?w=80', price: 14.99 },
      { name: 'Pasta Carbonara', image: 'https://images.unsplash.com/photo-1612874742237-6526221588e3?w=80', price: 11.99 },
      { name: 'Tacos', image: 'https://images.unsplash.com/photo-1551504734-5ee1c4a1479b?w=80', price: 8.49 },
      { name: 'Smoothie Bowl', image: 'https://images.unsplash.com/photo-1590301157890-4810ed352733?w=80', price: 6.99 },
      { name: 'Iced Coffee', image: 'https://images.unsplash.com/photo-1517487881594-2787fef5ebf7?w=80', price: 4.50 }
    ];

    const addresses = [
      '123 Campus Drive, Room 204',
      '456 University Ave, Apt 3B',
      '789 College St, Building A',
      '321 Student Hall, Room 501',
      '654 Dorm Circle, Suite 12'
    ];

    const specialInstructions = [
      'Please ring the doorbell',
      'Leave at the door',
      'No onions please',
      'Extra sauce on the side',
      'Call when you arrive'
    ];

    const mockOrders: Order[] = [
      // Pending Orders (3)
      {
        id: '1',
        orderNumber: 'ORD-2025-001',
        userId: 'user1',
        status: OrderStatus.Pending,
        deliveryMethod: DeliveryMethod.Pickup,
        paymentStatus: PaymentStatus.Pending,
        subtotal: 12.99,
        taxAmount: 1.04,
        deliveryFee: 0,
        totalAmount: 14.03,
        estimatedDeliveryTime: new Date(Date.now() + 30 * 60000),
        placedAt: new Date(Date.now() - 2 * 60000),
        items: [
          {
            id: '1',
            orderId: '1',
            menuItemId: '1',
            menuItemName: menuItems[0].name,
            menuItemImage: menuItems[0].image,
            quantity: 1,
            unitPrice: menuItems[0].price,
            totalPrice: menuItems[0].price
          }
        ]
      },
      {
        id: '2',
        orderNumber: 'ORD-2025-002',
        userId: 'user1',
        status: OrderStatus.Pending,
        deliveryMethod: DeliveryMethod.Delivery,
        deliveryAddress: addresses[0],
        paymentStatus: PaymentStatus.Pending,
        subtotal: 21.98,
        taxAmount: 1.76,
        deliveryFee: 3.99,
        totalAmount: 27.73,
        estimatedDeliveryTime: new Date(Date.now() + 35 * 60000),
        placedAt: new Date(Date.now() - 5 * 60000),
        items: [
          {
            id: '2',
            orderId: '2',
            menuItemId: '2',
            menuItemName: menuItems[1].name,
            menuItemImage: menuItems[1].image,
            quantity: 1,
            unitPrice: menuItems[1].price,
            totalPrice: menuItems[1].price
          },
          {
            id: '3',
            orderId: '2',
            menuItemId: '3',
            menuItemName: menuItems[2].name,
            menuItemImage: menuItems[2].image,
            quantity: 1,
            unitPrice: menuItems[2].price,
            totalPrice: menuItems[2].price
          }
        ]
      },
      {
        id: '3',
        orderNumber: 'ORD-2025-003',
        userId: 'user1',
        status: OrderStatus.Pending,
        deliveryMethod: DeliveryMethod.Pickup,
        paymentStatus: PaymentStatus.Paid,
        subtotal: 14.99,
        taxAmount: 1.20,
        deliveryFee: 0,
        totalAmount: 16.19,
        estimatedDeliveryTime: new Date(Date.now() + 25 * 60000),
        placedAt: new Date(Date.now() - 3 * 60000),
        items: [
          {
            id: '4',
            orderId: '3',
            menuItemId: '6',
            menuItemName: menuItems[5].name,
            menuItemImage: menuItems[5].image,
            quantity: 1,
            unitPrice: menuItems[5].price,
            totalPrice: menuItems[5].price
          }
        ]
      },
      // Confirmed Orders (3)
      {
        id: '4',
        orderNumber: 'ORD-2025-004',
        userId: 'user1',
        status: OrderStatus.Confirmed,
        deliveryMethod: DeliveryMethod.Delivery,
        deliveryAddress: addresses[1],
        paymentStatus: PaymentStatus.Paid,
        subtotal: 23.98,
        taxAmount: 1.92,
        deliveryFee: 3.99,
        totalAmount: 29.89,
        estimatedDeliveryTime: new Date(Date.now() + 28 * 60000),
        placedAt: new Date(Date.now() - 8 * 60000),
        confirmedAt: new Date(Date.now() - 6 * 60000),
        items: [
          {
            id: '5',
            orderId: '4',
            menuItemId: '7',
            menuItemName: menuItems[6].name,
            menuItemImage: menuItems[6].image,
            quantity: 2,
            unitPrice: menuItems[6].price,
            totalPrice: menuItems[6].price * 2
          }
        ],
        specialInstructions: specialInstructions[0]
      },
      {
        id: '5',
        orderNumber: 'ORD-2025-005',
        userId: 'user1',
        status: OrderStatus.Confirmed,
        deliveryMethod: DeliveryMethod.Pickup,
        paymentStatus: PaymentStatus.Paid,
        subtotal: 11.48,
        taxAmount: 0.92,
        deliveryFee: 0,
        totalAmount: 12.40,
        estimatedDeliveryTime: new Date(Date.now() + 20 * 60000),
        placedAt: new Date(Date.now() - 10 * 60000),
        confirmedAt: new Date(Date.now() - 7 * 60000),
        items: [
          {
            id: '6',
            orderId: '5',
            menuItemId: '4',
            menuItemName: menuItems[3].name,
            menuItemImage: menuItems[3].image,
            quantity: 1,
            unitPrice: menuItems[3].price,
            totalPrice: menuItems[3].price
          },
          {
            id: '7',
            orderId: '5',
            menuItemId: '9',
            menuItemName: menuItems[8].name,
            menuItemImage: menuItems[8].image,
            quantity: 1,
            unitPrice: menuItems[8].price,
            totalPrice: menuItems[8].price
          }
        ]
      },
      {
        id: '6',
        orderNumber: 'ORD-2025-006',
        userId: 'user1',
        status: OrderStatus.Confirmed,
        deliveryMethod: DeliveryMethod.Delivery,
        deliveryAddress: addresses[2],
        paymentStatus: PaymentStatus.Paid,
        subtotal: 16.98,
        taxAmount: 1.36,
        deliveryFee: 3.99,
        totalAmount: 22.33,
        estimatedDeliveryTime: new Date(Date.now() + 32 * 60000),
        placedAt: new Date(Date.now() - 12 * 60000),
        confirmedAt: new Date(Date.now() - 9 * 60000),
        items: [
          {
            id: '8',
            orderId: '6',
            menuItemId: '8',
            menuItemName: menuItems[7].name,
            menuItemImage: menuItems[7].image,
            quantity: 2,
            unitPrice: menuItems[7].price,
            totalPrice: menuItems[7].price * 2
          }
        ],
        specialInstructions: specialInstructions[1]
      },
      // Preparing Orders (4)
      {
        id: '7',
        orderNumber: 'ORD-2025-007',
        userId: 'user1',
        status: OrderStatus.Preparing,
        deliveryMethod: DeliveryMethod.Pickup,
        paymentStatus: PaymentStatus.Paid,
        subtotal: 25.98,
        taxAmount: 2.08,
        deliveryFee: 0,
        totalAmount: 28.06,
        estimatedDeliveryTime: new Date(Date.now() + 15 * 60000),
        placedAt: new Date(Date.now() - 15 * 60000),
        confirmedAt: new Date(Date.now() - 13 * 60000),
        items: [
          {
            id: '9',
            orderId: '7',
            menuItemId: '1',
            menuItemName: menuItems[0].name,
            menuItemImage: menuItems[0].image,
            quantity: 2,
            unitPrice: menuItems[0].price,
            totalPrice: menuItems[0].price * 2
          }
        ]
      },
      {
        id: '8',
        orderNumber: 'ORD-2025-008',
        userId: 'user1',
        status: OrderStatus.Preparing,
        deliveryMethod: DeliveryMethod.Delivery,
        deliveryAddress: addresses[3],
        paymentStatus: PaymentStatus.Paid,
        subtotal: 19.98,
        taxAmount: 1.60,
        deliveryFee: 3.99,
        totalAmount: 25.57,
        estimatedDeliveryTime: new Date(Date.now() + 18 * 60000),
        placedAt: new Date(Date.now() - 18 * 60000),
        confirmedAt: new Date(Date.now() - 15 * 60000),
        items: [
          {
            id: '10',
            orderId: '8',
            menuItemId: '2',
            menuItemName: menuItems[1].name,
            menuItemImage: menuItems[1].image,
            quantity: 2,
            unitPrice: menuItems[1].price,
            totalPrice: menuItems[1].price * 2
          }
        ],
        specialInstructions: specialInstructions[2]
      },
      {
        id: '9',
        orderNumber: 'ORD-2025-009',
        userId: 'user1',
        status: OrderStatus.Preparing,
        deliveryMethod: DeliveryMethod.Pickup,
        paymentStatus: PaymentStatus.Paid,
        subtotal: 20.48,
        taxAmount: 1.64,
        deliveryFee: 0,
        totalAmount: 22.12,
        estimatedDeliveryTime: new Date(Date.now() + 12 * 60000),
        placedAt: new Date(Date.now() - 20 * 60000),
        confirmedAt: new Date(Date.now() - 17 * 60000),
        items: [
          {
            id: '11',
            orderId: '9',
            menuItemId: '3',
            menuItemName: menuItems[2].name,
            menuItemImage: menuItems[2].image,
            quantity: 1,
            unitPrice: menuItems[2].price,
            totalPrice: menuItems[2].price
          },
          {
            id: '12',
            orderId: '9',
            menuItemId: '7',
            menuItemName: menuItems[6].name,
            menuItemImage: menuItems[6].image,
            quantity: 1,
            unitPrice: menuItems[6].price,
            totalPrice: menuItems[6].price
          }
        ]
      },
      {
        id: '10',
        orderNumber: 'ORD-2025-010',
        userId: 'user1',
        status: OrderStatus.Preparing,
        deliveryMethod: DeliveryMethod.Delivery,
        deliveryAddress: addresses[4],
        paymentStatus: PaymentStatus.Paid,
        subtotal: 29.97,
        taxAmount: 2.40,
        deliveryFee: 3.99,
        totalAmount: 36.36,
        estimatedDeliveryTime: new Date(Date.now() + 22 * 60000),
        placedAt: new Date(Date.now() - 22 * 60000),
        confirmedAt: new Date(Date.now() - 19 * 60000),
        items: [
          {
            id: '13',
            orderId: '10',
            menuItemId: '6',
            menuItemName: menuItems[5].name,
            menuItemImage: menuItems[5].image,
            quantity: 2,
            unitPrice: menuItems[5].price,
            totalPrice: menuItems[5].price * 2
          }
        ],
        specialInstructions: specialInstructions[3]
      },
      // Ready Orders (3)
      {
        id: '11',
        orderNumber: 'ORD-2025-011',
        userId: 'user1',
        status: OrderStatus.Ready,
        deliveryMethod: DeliveryMethod.Pickup,
        paymentStatus: PaymentStatus.Paid,
        subtotal: 17.48,
        taxAmount: 1.40,
        deliveryFee: 0,
        totalAmount: 18.88,
        estimatedDeliveryTime: new Date(Date.now() + 5 * 60000),
        placedAt: new Date(Date.now() - 25 * 60000),
        confirmedAt: new Date(Date.now() - 22 * 60000),
        items: [
          {
            id: '14',
            orderId: '11',
            menuItemId: '8',
            menuItemName: menuItems[7].name,
            menuItemImage: menuItems[7].image,
            quantity: 1,
            unitPrice: menuItems[7].price,
            totalPrice: menuItems[7].price
          },
          {
            id: '15',
            orderId: '11',
            menuItemId: '3',
            menuItemName: menuItems[2].name,
            menuItemImage: menuItems[2].image,
            quantity: 1,
            unitPrice: menuItems[2].price,
            totalPrice: menuItems[2].price
          }
        ]
      },
      {
        id: '12',
        orderNumber: 'ORD-2025-012',
        userId: 'user1',
        status: OrderStatus.Ready,
        deliveryMethod: DeliveryMethod.Delivery,
        deliveryAddress: addresses[0],
        paymentStatus: PaymentStatus.Paid,
        subtotal: 13.98,
        taxAmount: 1.12,
        deliveryFee: 3.99,
        totalAmount: 19.09,
        estimatedDeliveryTime: new Date(Date.now() + 8 * 60000),
        placedAt: new Date(Date.now() - 28 * 60000),
        confirmedAt: new Date(Date.now() - 25 * 60000),
        items: [
          {
            id: '16',
            orderId: '12',
            menuItemId: '4',
            menuItemName: menuItems[3].name,
            menuItemImage: menuItems[3].image,
            quantity: 1,
            unitPrice: menuItems[3].price,
            totalPrice: menuItems[3].price
          },
          {
            id: '17',
            orderId: '12',
            menuItemId: '9',
            menuItemName: menuItems[8].name,
            menuItemImage: menuItems[8].image,
            quantity: 1,
            unitPrice: menuItems[8].price,
            totalPrice: menuItems[8].price
          }
        ],
        specialInstructions: specialInstructions[4]
      },
      {
        id: '13',
        orderNumber: 'ORD-2025-013',
        userId: 'user1',
        status: OrderStatus.Ready,
        deliveryMethod: DeliveryMethod.Pickup,
        paymentStatus: PaymentStatus.Paid,
        subtotal: 9.00,
        taxAmount: 0.72,
        deliveryFee: 0,
        totalAmount: 9.72,
        estimatedDeliveryTime: new Date(Date.now() + 3 * 60000),
        placedAt: new Date(Date.now() - 30 * 60000),
        confirmedAt: new Date(Date.now() - 27 * 60000),
        items: [
          {
            id: '18',
            orderId: '13',
            menuItemId: '10',
            menuItemName: menuItems[9].name,
            menuItemImage: menuItems[9].image,
            quantity: 2,
            unitPrice: menuItems[9].price,
            totalPrice: menuItems[9].price * 2
          }
        ]
      },
      // In Delivery Orders (2)
      {
        id: '14',
        orderNumber: 'ORD-2025-014',
        userId: 'user1',
        status: OrderStatus.InDelivery,
        deliveryMethod: DeliveryMethod.Delivery,
        deliveryAddress: addresses[1],
        paymentStatus: PaymentStatus.Paid,
        subtotal: 24.98,
        taxAmount: 2.00,
        deliveryFee: 3.99,
        totalAmount: 30.97,
        estimatedDeliveryTime: new Date(Date.now() + 10 * 60000),
        placedAt: new Date(Date.now() - 35 * 60000),
        confirmedAt: new Date(Date.now() - 32 * 60000),
        items: [
          {
            id: '19',
            orderId: '14',
            menuItemId: '2',
            menuItemName: menuItems[1].name,
            menuItemImage: menuItems[1].image,
            quantity: 1,
            unitPrice: menuItems[1].price,
            totalPrice: menuItems[1].price
          },
          {
            id: '20',
            orderId: '14',
            menuItemId: '6',
            menuItemName: menuItems[5].name,
            menuItemImage: menuItems[5].image,
            quantity: 1,
            unitPrice: menuItems[5].price,
            totalPrice: menuItems[5].price
          }
        ],
        specialInstructions: specialInstructions[0]
      },
      {
        id: '15',
        orderNumber: 'ORD-2025-015',
        userId: 'user1',
        status: OrderStatus.InDelivery,
        deliveryMethod: DeliveryMethod.Delivery,
        deliveryAddress: addresses[2],
        paymentStatus: PaymentStatus.Paid,
        subtotal: 18.48,
        taxAmount: 1.48,
        deliveryFee: 3.99,
        totalAmount: 23.95,
        estimatedDeliveryTime: new Date(Date.now() + 12 * 60000),
        placedAt: new Date(Date.now() - 38 * 60000),
        confirmedAt: new Date(Date.now() - 35 * 60000),
        items: [
          {
            id: '21',
            orderId: '15',
            menuItemId: '7',
            menuItemName: menuItems[6].name,
            menuItemImage: menuItems[6].image,
            quantity: 1,
            unitPrice: menuItems[6].price,
            totalPrice: menuItems[6].price
          },
          {
            id: '22',
            orderId: '15',
            menuItemId: '4',
            menuItemName: menuItems[3].name,
            menuItemImage: menuItems[3].image,
            quantity: 1,
            unitPrice: menuItems[3].price,
            totalPrice: menuItems[3].price
          }
        ],
        specialInstructions: specialInstructions[1]
      },
      // Completed Orders (3)
      {
        id: '16',
        orderNumber: 'ORD-2025-016',
        userId: 'user1',
        status: OrderStatus.Completed,
        deliveryMethod: DeliveryMethod.Pickup,
        paymentStatus: PaymentStatus.Paid,
        subtotal: 15.99,
        taxAmount: 1.28,
        deliveryFee: 0,
        totalAmount: 17.27,
        placedAt: new Date(Date.now() - 2 * 24 * 60 * 60000),
        confirmedAt: new Date(Date.now() - 2 * 24 * 60 * 60000 + 5 * 60000),
        completedAt: new Date(Date.now() - 2 * 24 * 60 * 60000 + 30 * 60000),
        items: [
          {
            id: '23',
            orderId: '16',
            menuItemId: '3',
            menuItemName: menuItems[2].name,
            menuItemImage: menuItems[2].image,
            quantity: 1,
            unitPrice: menuItems[2].price,
            totalPrice: menuItems[2].price
          },
          {
            id: '24',
            orderId: '16',
            menuItemId: '5',
            menuItemName: menuItems[4].name,
            menuItemImage: menuItems[4].image,
            quantity: 2,
            unitPrice: menuItems[4].price,
            totalPrice: menuItems[4].price * 2
          }
        ]
      },
      {
        id: '17',
        orderNumber: 'ORD-2025-017',
        userId: 'user1',
        status: OrderStatus.Completed,
        deliveryMethod: DeliveryMethod.Delivery,
        deliveryAddress: addresses[3],
        paymentStatus: PaymentStatus.Paid,
        subtotal: 27.97,
        taxAmount: 2.24,
        deliveryFee: 3.99,
        totalAmount: 34.20,
        placedAt: new Date(Date.now() - 3 * 24 * 60 * 60000),
        confirmedAt: new Date(Date.now() - 3 * 24 * 60 * 60000 + 7 * 60000),
        completedAt: new Date(Date.now() - 3 * 24 * 60 * 60000 + 45 * 60000),
        items: [
          {
            id: '25',
            orderId: '17',
            menuItemId: '1',
            menuItemName: menuItems[0].name,
            menuItemImage: menuItems[0].image,
            quantity: 1,
            unitPrice: menuItems[0].price,
            totalPrice: menuItems[0].price
          },
          {
            id: '26',
            orderId: '17',
            menuItemId: '6',
            menuItemName: menuItems[5].name,
            menuItemImage: menuItems[5].image,
            quantity: 1,
            unitPrice: menuItems[5].price,
            totalPrice: menuItems[5].price
          }
        ],
        specialInstructions: specialInstructions[2]
      },
      {
        id: '18',
        orderNumber: 'ORD-2025-018',
        userId: 'user1',
        status: OrderStatus.Completed,
        deliveryMethod: DeliveryMethod.Pickup,
        paymentStatus: PaymentStatus.Paid,
        subtotal: 22.48,
        taxAmount: 1.80,
        deliveryFee: 0,
        totalAmount: 24.28,
        placedAt: new Date(Date.now() - 5 * 24 * 60 * 60000),
        confirmedAt: new Date(Date.now() - 5 * 24 * 60 * 60000 + 6 * 60000),
        completedAt: new Date(Date.now() - 5 * 24 * 60 * 60000 + 28 * 60000),
        items: [
          {
            id: '27',
            orderId: '18',
            menuItemId: '8',
            menuItemName: menuItems[7].name,
            menuItemImage: menuItems[7].image,
            quantity: 2,
            unitPrice: menuItems[7].price,
            totalPrice: menuItems[7].price * 2
          },
          {
            id: '28',
            orderId: '18',
            menuItemId: '5',
            menuItemName: menuItems[4].name,
            menuItemImage: menuItems[4].image,
            quantity: 1,
            unitPrice: menuItems[4].price,
            totalPrice: menuItems[4].price
          }
        ]
      },
      // Cancelled Orders (2)
      {
        id: '19',
        orderNumber: 'ORD-2025-019',
        userId: 'user1',
        status: OrderStatus.Cancelled,
        deliveryMethod: DeliveryMethod.Delivery,
        deliveryAddress: addresses[4],
        paymentStatus: PaymentStatus.Refunded,
        subtotal: 19.98,
        taxAmount: 1.60,
        deliveryFee: 3.99,
        totalAmount: 25.57,
        placedAt: new Date(Date.now() - 1 * 24 * 60 * 60000),
        confirmedAt: new Date(Date.now() - 1 * 24 * 60 * 60000 + 5 * 60000),
        items: [
          {
            id: '29',
            orderId: '19',
            menuItemId: '2',
            menuItemName: menuItems[1].name,
            menuItemImage: menuItems[1].image,
            quantity: 2,
            unitPrice: menuItems[1].price,
            totalPrice: menuItems[1].price * 2
          }
        ],
        specialInstructions: specialInstructions[3]
      },
      {
        id: '20',
        orderNumber: 'ORD-2025-020',
        userId: 'user1',
        status: OrderStatus.Cancelled,
        deliveryMethod: DeliveryMethod.Pickup,
        paymentStatus: PaymentStatus.Refunded,
        subtotal: 11.99,
        taxAmount: 0.96,
        deliveryFee: 0,
        totalAmount: 12.95,
        placedAt: new Date(Date.now() - 6 * 24 * 60 * 60000),
        items: [
          {
            id: '30',
            orderId: '20',
            menuItemId: '7',
            menuItemName: menuItems[6].name,
            menuItemImage: menuItems[6].image,
            quantity: 1,
            unitPrice: menuItems[6].price,
            totalPrice: menuItems[6].price
          }
        ]
      }
    ];

    this.orders.set(mockOrders);
  }
}

