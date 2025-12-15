import { Component, input, output, signal, effect, inject, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DialogModule } from 'primeng/dialog';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MenuItem } from '../../../menu/models/menu-item.model';
import { MenuService } from '../../../menu/services/menu.service';

@Component({
  selector: 'app-menu-item-form-modal',
  imports: [CommonModule, DialogModule, FormsModule, ReactiveFormsModule],
  templateUrl: './menu-item-form-modal.component.html',
  styleUrl: './menu-item-form-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MenuItemFormModalComponent {
  visible = input.required<boolean>();
  menuItem = input<MenuItem | null>(null);
  close = output<void>();

  private fb = inject(FormBuilder);
  private menuService = inject(MenuService);

  isVisible = signal(false);
  categories = this.menuService.categories;
  menuLoading = this.menuService.loading;
  menuError = this.menuService.error;
  
  private isSubmitting = signal(false);
  private previousLoadingState = signal(false);

  form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    categoryId: [null as string | null],
    price: [0, [Validators.required, Validators.min(0)]],
    description: [''],
    isAvailable: [true],
    preparationTimeMinutes: [null as number | null],
    calories: [null as number | null]
  });

  constructor() {
    effect(() => {
      this.isVisible.set(this.visible());
      if (this.visible()) {
        const item = this.menuItem();
        if (item) {
          // Find category ID by name
          const category = this.categories().find(c => c.name === item.categoryName);
          
          this.form.patchValue({
            name: item.name,
            categoryId: category?.id ?? null,
            price: item.price,
            description: item.description,
            isAvailable: item.isAvailable,
            preparationTimeMinutes: item.preparationTimeMinutes ?? null,
            calories: item.calories ?? null
          });
        } else {
          this.resetForm();
        }
      }
    });

    // Watch for loading state changes to close modal after successful submission
    effect(() => {
      const currentLoading = this.menuLoading();
      const wasLoading = this.previousLoadingState();
      const submitting = this.isSubmitting();

      // If we were loading and now we're not, and we're submitting, and there's no error
      if (wasLoading && !currentLoading && submitting && !this.menuError()) {
        this.isSubmitting.set(false);
        this.onClose();
      }

      this.previousLoadingState.set(currentLoading);
    });
  }

  onDialogHide(): void {
    this.resetForm();
    this.close.emit();
  }

  onClose(): void {
    this.isVisible.set(false);
  }

  submitForm(): void {
    if (this.form.invalid) return;

    const payload = this.formValueToPayload();
    const item = this.menuItem();

    this.isSubmitting.set(true);

    if (item) {
      this.menuService.updateMenuItem(item.id, payload);
    } else {
      this.menuService.createMenuItem(payload);
    }
  }

  private formValueToPayload() {
    const raw = this.form.value;
    return {
      name: raw.name ?? '',
      categoryId: raw.categoryId ?? undefined,
      price: raw.price ?? 0,
      description: raw.description ?? '',
      isAvailable: raw.isAvailable ?? true,
      preparationTimeMinutes: raw.preparationTimeMinutes ?? undefined,
      calories: raw.calories ?? undefined
    };
  }

  private resetForm(): void {
    this.isSubmitting.set(false);
    this.form.reset({
      name: '',
      categoryId: null,
      price: 0,
      description: '',
      isAvailable: true,
      preparationTimeMinutes: null,
      calories: null
    });
  }
}