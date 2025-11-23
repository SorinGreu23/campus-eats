import { Component, signal, inject, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DrawerModule } from 'primeng/drawer';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';
import { TooltipModule } from 'primeng/tooltip';
import { FormsModule } from '@angular/forms';
import { CartService } from '../../services/cart.service';

@Component({
  selector: 'app-cart',
  imports: [
    CommonModule,
    DrawerModule,
    ButtonModule,
    InputNumberModule,
    TooltipModule,
    FormsModule
  ],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CartComponent {
  cartService = inject(CartService);
  visible = signal(false);

  showCart(): void {
    this.visible.set(true);
  }

  hideCart(): void {
    this.visible.set(false);
  }

  updateQuantity(itemId: string, quantity: number | null): void {
    if (quantity !== null && quantity >= 0) {
      this.cartService.updateQuantity(itemId, quantity);
    }
  }

  removeItem(itemId: string): void {
    this.cartService.removeItem(itemId);
  }

  clearCart(): void {
    this.cartService.clearCart();
  }

  checkout(): void {
    // TODO: Implement checkout logic
    console.log('Proceeding to checkout...');
    this.hideCart();
  }
}
