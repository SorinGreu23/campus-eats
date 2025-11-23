import { TestBed } from '@angular/core/testing';
import { CartService } from './cart.service';
import { MenuItem } from '../../features/menu/models/menu-item.model';

describe('CartService', () => {
  let service: CartService;

  const mockMenuItem: MenuItem = {
    id: '1',
    name: 'Test Pizza',
    description: 'Test description',
    price: 10.99,
    isAvailable: true
  };

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CartService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should add item to cart', () => {
    service.addItem(mockMenuItem, 1);
    expect(service.items().length).toBe(1);
    expect(service.itemCount()).toBe(1);
  });

  it('should update quantity when adding existing item', () => {
    service.addItem(mockMenuItem, 1);
    service.addItem(mockMenuItem, 2);
    expect(service.items().length).toBe(1);
    expect(service.itemCount()).toBe(3);
  });

  it('should calculate subtotal correctly', () => {
    service.addItem(mockMenuItem, 2);
    expect(service.subtotal()).toBe(21.98);
  });

  it('should calculate tax correctly', () => {
    service.addItem(mockMenuItem, 1);
    const expectedTax = 10.99 * 0.08;
    expect(service.tax()).toBeCloseTo(expectedTax, 2);
  });

  it('should calculate total correctly', () => {
    service.addItem(mockMenuItem, 1);
    const expectedTotal = 10.99 * 1.08;
    expect(service.total()).toBeCloseTo(expectedTotal, 2);
  });

  it('should remove item from cart', () => {
    service.addItem(mockMenuItem, 1);
    service.removeItem(mockMenuItem.id);
    expect(service.items().length).toBe(0);
  });

  it('should update item quantity', () => {
    service.addItem(mockMenuItem, 1);
    service.updateQuantity(mockMenuItem.id, 5);
    expect(service.itemCount()).toBe(5);
  });

  it('should remove item when quantity is 0', () => {
    service.addItem(mockMenuItem, 1);
    service.updateQuantity(mockMenuItem.id, 0);
    expect(service.items().length).toBe(0);
  });

  it('should clear cart', () => {
    service.addItem(mockMenuItem, 1);
    service.clearCart();
    expect(service.items().length).toBe(0);
  });
});
