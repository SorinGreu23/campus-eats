import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CartComponent } from './cart.component';
import { CartService } from '../../services/cart.service';

describe('CartComponent', () => {
  let component: CartComponent;
  let fixture: ComponentFixture<CartComponent>;
  let cartService: CartService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CartComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(CartComponent);
    component = fixture.componentInstance;
    cartService = TestBed.inject(CartService);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should show cart when showCart is called', () => {
    component.showCart();
    expect(component.visible()).toBe(true);
  });

  it('should hide cart when hideCart is called', () => {
    component.showCart();
    component.hideCart();
    expect(component.visible()).toBe(false);
  });

  it('should call cartService.updateQuantity when updateQuantity is called', () => {
    spyOn(cartService, 'updateQuantity');
    component.updateQuantity('1', 5);
    expect(cartService.updateQuantity).toHaveBeenCalledWith('1', 5);
  });

  it('should call cartService.removeItem when removeItem is called', () => {
    spyOn(cartService, 'removeItem');
    component.removeItem('1');
    expect(cartService.removeItem).toHaveBeenCalledWith('1');
  });

  it('should call cartService.clearCart when clearCart is called', () => {
    spyOn(cartService, 'clearCart');
    component.clearCart();
    expect(cartService.clearCart).toHaveBeenCalled();
  });

  it('should hide cart when checkout is called', () => {
    component.showCart();
    component.checkout();
    expect(component.visible()).toBe(false);
  });
});
