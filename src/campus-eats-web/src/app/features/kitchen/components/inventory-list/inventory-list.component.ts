import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { InventoryService } from '../../../../shared/services/inventory.service';
import { InventoryItem } from '../../../../shared/models/inventory.model';

@Component({
  selector: 'app-inventory-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './inventory-list.component.html',
  styleUrl: './inventory-list.component.scss'
})
export class InventoryListComponent implements OnInit {
  private inventoryService = inject(InventoryService);

  items = this.inventoryService.items;
  loading = this.inventoryService.loading;
  error = this.inventoryService.error;

  showRestockModal = signal(false);
  selectedItem = signal<InventoryItem | null>(null);
  restockQuantity = signal<number>(0);
  restockReason = signal<string>('');

  ngOnInit(): void {
    this.inventoryService.loadInventoryItems();
  }

  refresh(): void {
    this.inventoryService.loadInventoryItems(true);
  }

  openRestockModal(item: InventoryItem): void {
    this.selectedItem.set(item);
    this.restockQuantity.set(0);
    this.restockReason.set('');
    this.showRestockModal.set(true);
  }

  closeRestockModal(): void {
    this.showRestockModal.set(false);
    this.selectedItem.set(null);
  }

  submitRestock(): void {
    const item = this.selectedItem();
    const quantity = this.restockQuantity();
    
    if (!item || quantity <= 0) {
      alert('Please enter a valid quantity');
      return;
    }

    this.inventoryService.restockItem(item.id, {
      quantity: quantity,
      reason: this.restockReason() || undefined
    });

    this.closeRestockModal();
  }

  getStockStatusClass(item: InventoryItem): string {
    if (item.isOutOfStock) return 'out-of-stock';
    if (item.isLowStock) return 'low-stock';
    return 'in-stock';
  }

  getStockStatusLabel(item: InventoryItem): string {
    if (item.isOutOfStock) return 'Out of Stock';
    if (item.isLowStock) return 'Low Stock';
    return 'In Stock';
  }
}
