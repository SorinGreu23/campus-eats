import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MenuItemCardComponent } from './menu-item-card.component';
import { CartService } from '../../../../shared/services/cart.service';
import { MenuItem } from '../../models/menu-item.model';
import { signal } from '@angular/core';

describe('MenuItemCardComponent', () => {
  let component: MenuItemCardComponent;
  let fixture: ComponentFixture<MenuItemCardComponent>;
  let cartService: CartService;

  const mockMenuItem: MenuItem = {
    id: '1',
    name: 'Test Pizza',
    description: 'Delicious test pizza',
    price: 12.99,
    isAvailable: true,
    calories: 280
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MenuItemCardComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(MenuItemCardComponent);
    component = fixture.componentInstance;
    cartService = TestBed.inject(CartService);
    
    // Set the required input
    fixture.componentRef.setInput('menuItem', mockMenuItem);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should add item to cart when addToCart is called', () => {
    spyOn(cartService, 'addItem');
    
    component.addToCart();
    
    expect(cartService.addItem).toHaveBeenCalledWith(mockMenuItem, 1);
    expect(component.isAdding()).toBe(true);
  });

  it('should reset isAdding state after adding to cart', (done) => {
    component.addToCart();
    expect(component.isAdding()).toBe(true);
    
    setTimeout(() => {
      expect(component.isAdding()).toBe(false);
      done();
    }, 900);
  });

  it('should display menu item information', () => {
    const compiled = fixture.nativeElement;
    
    expect(compiled.querySelector('.item-name').textContent).toContain('Test Pizza');
    expect(compiled.querySelector('.item-price').textContent).toContain('12.99');
    expect(compiled.querySelector('.item-description').textContent).toContain('Delicious test pizza');
  });
});
