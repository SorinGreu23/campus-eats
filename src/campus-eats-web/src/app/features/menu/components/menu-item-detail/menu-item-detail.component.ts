import { Component, input, output, signal, effect, inject, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DialogModule } from 'primeng/dialog';
import { MenuItem } from '../../models/menu-item.model';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-menu-item-detail',
  imports: [CommonModule, DialogModule],
  templateUrl: './menu-item-detail.component.html',
  styleUrl: './menu-item-detail.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MenuItemDetailComponent {
  visible = input.required<boolean>();
  menuItem = input<MenuItem | null>(null);
  close = output<void>();
  addToCart = output<MenuItem>();

  private messageService = inject(MessageService);

  isVisible = signal(false);
  quantity = signal(1);

  constructor() {
    effect(() => {
      this.isVisible.set(this.visible());
      if (this.visible()) {
        this.quantity.set(1);
      }
    });
  }

  onDialogHide(): void {
    this.close.emit();
  }

  onClose(): void {
    this.isVisible.set(false);
  }

  increaseQuantity(): void {
    this.quantity.update(q => q + 1);
  }

  decreaseQuantity(): void {
    this.quantity.update(q => Math.max(1, q - 1));
  }

  onAddToCart(): void {
    const item = this.menuItem();
    const qty = this.quantity();
    if (item && item.isAvailable) {
      for (let i = 0; i < qty; i++) {
        this.addToCart.emit(item);
      }
      
      // Show single notification with quantity
      this.messageService.add({
        severity: 'success',
        summary: 'Added to Cart!',
        detail: qty > 1 
          ? `${qty}x ${item.name} added to your cart`
          : `${item.name} has been added to your cart`,
        life: 3000
      });
      
      this.isVisible.set(false);
    }
  }

  getTotalPrice(): number {
    const item = this.menuItem();
    return item ? item.price * this.quantity() : 0;
  }
}
