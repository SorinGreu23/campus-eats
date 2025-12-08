import { Component, computed, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { ChipModule } from 'primeng/chip';
import { InputTextModule } from 'primeng/inputtext';
import { MenuService } from '../../services/menu.service';
import { MenuItemCardComponent } from '../menu-item-card/menu-item-card.component';
import { MenuItemDetailComponent } from '../menu-item-detail/menu-item-detail.component';
import { MenuItem } from '../../models/menu-item.model';
import { CartService } from '../../../../shared/services/cart.service';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-menu-list',
  imports: [
    ChipModule,
    InputTextModule,
    MenuItemCardComponent,
    MenuItemDetailComponent
  ],
  templateUrl: './menu-list.component.html',
  styleUrl: './menu-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MenuListComponent {
  private menuService = inject(MenuService);
  private cartService = inject(CartService);
  private messageService = inject(MessageService);

  searchTerm = signal('');
  selectedCategoryId = signal<string | null>(null);
  showDetailModal = signal(false);
  selectedMenuItem = signal<MenuItem | null>(null);

  categories = this.menuService.categories;
  
  filteredMenuItems = computed(() => {
    const categoryId = this.selectedCategoryId();
    const search = this.searchTerm().toLowerCase();
    let items = this.menuService.menuItems();

    if (categoryId) {
      items = items.filter(item => item.categoryId === categoryId);
    }

    if (search) {
      items = items.filter(item =>
        item.name.toLowerCase().includes(search) ||
        item.description?.toLowerCase().includes(search)
      );
    }

    return items;
  });

  selectCategory(categoryId: string | null): void {
    this.selectedCategoryId.set(categoryId);
  }

  onSearchChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.searchTerm.set(input.value);
  }

  onMenuItemClick(menuItem: MenuItem): void {
    this.selectedMenuItem.set(menuItem);
    setTimeout(() => {
      this.showDetailModal.set(true);
    }, 0);
  }

  handleCloseDetail(): void {
    this.showDetailModal.set(false);
    setTimeout(() => {
      this.selectedMenuItem.set(null);
    }, 300);
  }

  handleAddToCart(menuItem: MenuItem): void {
    this.cartService.addItem(menuItem, 1);
  }
}
