import { Component, input, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { MenuItem } from '../../models/menu-item.model';
import { CartService } from '../../../../shared/services/cart.service';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-menu-item-card',
  imports: [ButtonModule, TagModule],
  templateUrl: './menu-item-card.component.html',
  styleUrl: './menu-item-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MenuItemCardComponent {
  menuItem = input.required<MenuItem>();
  
  private cartService = inject(CartService);
  private messageService = inject(MessageService);
  isAdding = signal(false);

  addToCart(): void {
    this.isAdding.set(true);
    this.cartService.addItem(this.menuItem(), 1);
    
    // Show success notification
    this.messageService.add({
      severity: 'success',
      summary: 'Added to Cart!',
      detail: `${this.menuItem().name} has been added to your cart`,
      life: 3000
    });
    
    // Reset animation state after a short delay
    setTimeout(() => {
      this.isAdding.set(false);
    }, 800);
  }
}
