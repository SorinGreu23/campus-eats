import { Component, computed, inject, signal } from '@angular/core';
import { ChipModule } from 'primeng/chip';
import { InputTextModule } from 'primeng/inputtext';
import { MenuService } from '../../services/menu.service';
import { MenuItemCardComponent } from '../menu-item-card/menu-item-card.component';

@Component({
  selector: 'app-menu-list',
  standalone: true,
  imports: [
    ChipModule,
    InputTextModule,
    MenuItemCardComponent
  ],
  templateUrl: './menu-list.component.html',
  styleUrl: './menu-list.component.scss'
})
export class MenuListComponent {
  private menuService = inject(MenuService);

  searchTerm = signal('');
  selectedCategoryId = signal<string | null>(null);

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

  selectCategory(categoryId: string | null) {
    this.selectedCategoryId.set(categoryId);
  }

  onSearchChange(event: Event) {
    const input = event.target as HTMLInputElement;
    this.searchTerm.set(input.value);
  }
}
