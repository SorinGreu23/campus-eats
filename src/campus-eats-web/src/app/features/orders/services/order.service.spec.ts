import { TestBed } from '@angular/core/testing';
import { OrderService } from './order.service';
import { OrderStatus } from '../models/order.model';

describe('OrderService', () => {
  let service: OrderService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(OrderService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should initialize with mock data', () => {
    service.initializeMockData();
    expect(service.allOrders().length).toBeGreaterThan(0);
  });

  it('should filter active orders correctly', () => {
    service.initializeMockData();
    const active = service.activeOrders();
    expect(active.every(o => o.status !== OrderStatus.Completed && o.status !== OrderStatus.Cancelled)).toBe(true);
  });

  it('should filter completed orders correctly', () => {
    service.initializeMockData();
    const completed = service.completedOrders();
    expect(completed.every(o => o.status === OrderStatus.Completed || o.status === OrderStatus.Cancelled)).toBe(true);
  });

  it('should get order by id', () => {
    service.initializeMockData();
    const orders = service.allOrders();
    const firstOrder = orders[0];
    const foundOrder = service.getOrderById(firstOrder.id);
    expect(foundOrder).toEqual(firstOrder);
  });
});
