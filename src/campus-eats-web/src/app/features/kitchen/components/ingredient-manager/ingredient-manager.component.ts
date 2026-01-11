import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MenuService } from '../../../menu/services/menu.service';
import { MenuItemIngredientService } from '../../../menu/services/menu-item-ingredient.service';
import { InventoryService } from '../../../../shared/services/inventory.service';

@Component({
  selector: 'app-ingredient-manager',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ingredient-manager.component.html',
  styleUrl: './ingredient-manager.component.scss'
})
export class IngredientManagerComponent implements OnInit {
  private readonly menuService = inject(MenuService);
  private readonly ingredientService = inject(MenuItemIngredientService);
  private readonly inventoryService = inject(InventoryService);

  menuItems = this.menuService.menuItems;
  ingredients = this.ingredientService.ingredients;
  inventoryItems = this.inventoryService.items;

  selectedMenuItemId = signal<string | null>(null);
  showAddModal = signal(false);
  selectedInventoryItemId = signal<string | null>(null);
  quantityRequired = signal<number>(1);
  editingIngredient = signal<{inventoryItemId: string, quantity: number} | null>(null);

  selectedMenuItem = computed(() => {
    const id = this.selectedMenuItemId();
    return id ? this.menuItems().find(m => m.id === id) : null;
  });

  menuItemsWithoutIngredients = computed(() => {
    return this.menuItems().filter(menuItem => {
      const hasIngredients = this.menuItems().some(m => {
        if (m.id === menuItem.id) {
          // Check if this menu item has ingredients loaded
          return this.ingredients().some(ing => ing.menuItemId === menuItem.id);
        }
        return false;
      });
      return !hasIngredients;
    });
  });

  availableInventoryItems = computed(() => {
    const currentIngredients = this.ingredients();
    return this.inventoryItems().filter(inv => 
      !currentIngredients.some(ing => ing.inventoryItemId === inv.id)
    );
  });

  selectedInventoryItemUnit = computed(() => {
    const itemId = this.selectedInventoryItemId();
    if (!itemId) return '';
    const item = this.availableInventoryItems().find(i => i.id === itemId);
    return item?.unit || '';
  });

  ngOnInit(): void {
    this.menuService.loadMenuItems();
    this.inventoryService.loadInventoryItems();
  }

  selectMenuItem(id: string): void {
    this.selectedMenuItemId.set(id);
    this.ingredientService.loadIngredients(id);
  }

  openAddModal(): void {
    this.showAddModal.set(true);
    this.selectedInventoryItemId.set(null);
    this.quantityRequired.set(1);
  }

  closeAddModal(): void {
    this.showAddModal.set(false);
    this.editingIngredient.set(null);
  }

  async addIngredient(): Promise<void> {
    const menuItemId = this.selectedMenuItemId();
    const inventoryItemId = this.selectedInventoryItemId();
    const quantity = this.quantityRequired();

    if (!menuItemId || !inventoryItemId || quantity <= 0) {
      return;
    }

    try {
      await this.ingredientService.addIngredient(menuItemId, {
        inventoryItemId,
        quantityRequired: quantity
      });
      this.closeAddModal();
    } catch (error) {
      console.error('Failed to add ingredient:', error);
      alert(error);
    }
  }

  startEdit(inventoryItemId: string, currentQuantity: number): void {
    this.editingIngredient.set({ inventoryItemId, quantity: currentQuantity });
  }

  cancelEdit(): void {
    this.editingIngredient.set(null);
  }

  async saveEdit(inventoryItemId: string): Promise<void> {
    const menuItemId = this.selectedMenuItemId();
    const editing = this.editingIngredient();

    if (!menuItemId || !editing) return;

    try {
      await this.ingredientService.updateIngredient(menuItemId, inventoryItemId, editing.quantity);
      this.editingIngredient.set(null);
    } catch (error) {
      console.error('Failed to update ingredient:', error);
      alert(error);
    }
  }

  async deleteIngredient(inventoryItemId: string): Promise<void> {
    const menuItemId = this.selectedMenuItemId();
    if (!menuItemId) return;

    if (!confirm('Are you sure you want to remove this ingredient?')) {
      return;
    }

    try {
      await this.ingredientService.deleteIngredient(menuItemId, inventoryItemId);
    } catch (error) {
      console.error('Failed to delete ingredient:', error);
      alert(error);
    }
  }

  updateEditQuantity(newQuantity: number): void {
    const editing = this.editingIngredient();
    if (editing) {
      this.editingIngredient.set({ ...editing, quantity: newQuantity });
    }
  }
}
