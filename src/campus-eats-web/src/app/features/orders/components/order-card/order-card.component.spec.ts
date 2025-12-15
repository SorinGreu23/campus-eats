import { ComponentFixture, TestBed } from '@angular/core/testing';
import { OrderCardComponent } from './order-card.component';
import { Order, OrderStatus, PaymentStatus, DeliveryMethod } from '../../models/order.model';

describe('OrderCardComponent', () => {
  let component: OrderCardComponent;
  let fixture: ComponentFixture<OrderCardComponent>;

  const mockOrder: Order = {
    id: '1',
    orderNumber: 'ORD-2025-001',
    userId: 'user1',
    status: OrderStatus.Preparing,
    orderType: DeliveryMethod.Pickup,
    paymentStatus: PaymentStatus.Paid,
    subtotal: 25.98,
    tax: 2.08,
    discount: 0,
    total: 28.06,
    placedAt: new Date(),
    items: []
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OrderCardComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(OrderCardComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('order', mockOrder);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should compute status configuration correctly', () => {
    const config = component.statusConfig();
    expect(config).toBeDefined();
    expect(config.label).toBe('Preparing');
  });

  it('should format date correctly', () => {
    const formatted = component.formattedDate();
    expect(formatted).toContain('Today');
  });
});
