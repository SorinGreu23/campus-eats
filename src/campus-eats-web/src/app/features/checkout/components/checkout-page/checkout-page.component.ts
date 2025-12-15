import { Component, OnInit, inject, signal, computed, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { RadioButtonModule } from 'primeng/radiobutton';
import { MessageService } from 'primeng/api';
import { CartService } from '../../../../shared/services/cart.service';
import { AuthStateService } from '../../../../shared/services/auth-state.service';
import { HttpClient, HttpHeaders } from '@angular/common/http';

const API_BASE_URL = 'http://localhost:5001/api';

interface CreateOrderItemRequest {
  menuItemId: string;
  quantity: number;
  specialInstructions?: string;
}

interface CreateOrderRequest {
  deliveryInstructions?: string;
  orderType?: string;
  items: CreateOrderItemRequest[];
}

@Component({
  selector: 'app-checkout-page',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ButtonModule,
    RadioButtonModule
  ],
  templateUrl: './checkout-page.component.html',
  styleUrl: './checkout-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CheckoutPageComponent implements OnInit {
  private cartService = inject(CartService);
  private authState = inject(AuthStateService);
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private http = inject(HttpClient);
  private messageService = inject(MessageService);

  cartItems = this.cartService.items;
  subtotal = this.cartService.subtotal;
  tax = computed(() => Math.round(this.subtotal() * 0.21 * 100) / 100); // 21% tax to match backend
  total = computed(() => Math.round((this.subtotal() + this.tax()) * 100) / 100);

  isSubmitting = signal(false);
  
  checkoutForm = this.fb.group({
    orderType: ['Pickup', Validators.required],
    deliveryInstructions: ['']
  });

  orderTypes = [
    { label: 'Pickup', value: 'Pickup' },
    { label: 'Delivery', value: 'Delivery' }
  ];

  ngOnInit(): void {
    // Redirect if cart is empty
    if (this.cartItems().length === 0) {
      this.router.navigate(['/menu']);
    }

    // Redirect to login if not authenticated
    if (!this.authState.isLoggedIn()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Login Required',
        detail: 'Please login to place an order'
      });
      this.router.navigate(['/login']);
    }
  }

  submitOrder(): void {
    if (this.checkoutForm.invalid || this.isSubmitting()) return;
    
    const token = this.authState.token();
    if (!token) {
      this.messageService.add({
        severity: 'error',
        summary: 'Authentication Error',
        detail: 'Please login to continue'
      });
      this.router.navigate(['/login']);
      return;
    }

    const formValue = this.checkoutForm.value;
    
    const orderRequest: CreateOrderRequest = {
      orderType: formValue.orderType || 'Pickup',
      deliveryInstructions: formValue.deliveryInstructions || undefined,
      items: this.cartItems().map(item => ({
        menuItemId: item.menuItem.id,
        quantity: item.quantity,
        specialInstructions: undefined
      }))
    };

    this.isSubmitting.set(true);

    const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });

    this.http.post(`${API_BASE_URL}/orders`, orderRequest, { headers }).subscribe({
      next: (response) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Order Placed!',
          detail: 'Your order has been successfully placed'
        });
        this.cartService.clearCart();
        this.router.navigate(['/orders']);
        this.isSubmitting.set(false);
      },
      error: (err) => {
        const message = err?.error?.error || err?.error?.title || 'Failed to place order';
        this.messageService.add({
          severity: 'error',
          summary: 'Order Failed',
          detail: message
        });
        this.isSubmitting.set(false);
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/menu']);
  }
}
