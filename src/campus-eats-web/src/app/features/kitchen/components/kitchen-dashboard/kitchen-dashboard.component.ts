import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MenuService } from '../../../menu/services/menu.service';
import { OrderService } from '../../../orders/services/order.service';
import { MenuItem } from '../../../menu/models/menu-item.model';
import { Order } from '../../../orders/models/order.model';

@Component({
  selector: 'app-kitchen-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './kitchen-dashboard.component.html',
  styleUrl: './kitchen-dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class KitchenDashboardComponent implements OnInit {
  private menuService = inject(MenuService);
  private orderService = inject(OrderService);
  private fb = inject(FormBuilder);

  menuItems = this.menuService.menuItems;
  menuLoading = this.menuService.loading;
  menuError = this.menuService.error;

  pendingOrders = this.orderService.pendingKitchenOrders;
  ordersLoading = this.orderService.kitchenLoading;
  ordersError = this.orderService.kitchenError;

  editingId = signal<string | null>(null);
  form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    price: [0, [Validators.required, Validators.min(0)]],
    description: [''],
    isAvailable: [true],
    preparationTimeMinutes: [null as number | null],
    calories: [null as number | null]
  });

  filteredMenu = computed(() => this.menuItems());

  ngOnInit(): void {
    this.menuService.loadMenuItems();
    this.orderService.loadPendingOrders(true);
  }

  submitForm(): void {
    if (this.form.invalid) return;

    const payload = this.formValueToPayload();

    if (this.editingId()) {
      this.menuService.updateMenuItem(this.editingId()!, payload);
    } else {
      this.menuService.createMenuItem(payload);
    }

    this.resetForm();
  }

  editItem(item: MenuItem): void {
    this.editingId.set(item.id);
    this.form.patchValue({
      name: item.name,
      price: item.price,
      description: item.description,
      isAvailable: item.isAvailable,
      preparationTimeMinutes: item.preparationTimeMinutes ?? null,
      calories: item.calories ?? null
    });
  }

  deleteItem(id: string): void {
    const confirmed = confirm('Delete this menu item?');
    if (!confirmed) return;
    this.menuService.deleteMenuItem(id);
  }

  markOrderComplete(orderId: string): void {
    this.orderService.completeOrder(orderId);
  }

  refresh(): void {
    this.menuService.loadMenuItems(true);
    this.orderService.loadPendingOrders(true);
  }

  cancelEdit(): void {
    this.resetForm();
  }

  private formValueToPayload() {
    const raw = this.form.value;
    return {
      name: raw.name ?? '',
      description: raw.description ?? undefined,
      price: Number(raw.price ?? 0),
      categoryId: null,
      imageUrl: null,
      preparationTimeMinutes: raw.preparationTimeMinutes ?? null,
      isAvailable: !!raw.isAvailable,
      calories: raw.calories ?? null,
      allergenIds: [],
      dietaryRestrictionIds: []
    };
  }

  private resetForm(): void {
    this.editingId.set(null);
    this.form.reset({
      name: '',
      price: 0,
      description: '',
      isAvailable: true,
      preparationTimeMinutes: null,
      calories: null
    });
  }
}
