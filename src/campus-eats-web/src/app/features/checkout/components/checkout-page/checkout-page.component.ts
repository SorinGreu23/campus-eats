import { Component, OnInit, inject, signal, computed, ChangeDetectionStrategy, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { RadioButtonModule } from 'primeng/radiobutton';
import { MessageService } from 'primeng/api';
import { CartService } from '../../../../shared/services/cart.service';
import { AuthStateService } from '../../../../shared/services/auth-state.service';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { PaymentService } from '../../../../shared/services/payment.service';
import { StripeElements } from '@stripe/stripe-js';

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
export class CheckoutPageComponent implements OnInit, AfterViewInit {
  @ViewChild('paymentElement') paymentElementRef!: ElementRef;

  private cartService = inject(CartService);
  private authState = inject(AuthStateService);
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private http = inject(HttpClient);
  private messageService = inject(MessageService);
  private paymentService = inject(PaymentService);

  cartItems = this.cartService.items;
  subtotal = this.cartService.subtotal;
  tax = computed(() => Math.round(this.subtotal() * 0.21 * 100) / 100);
  total = computed(() => Math.round((this.subtotal() + this.tax()) * 100) / 100);

  isSubmitting = signal(false);
  showPayment = signal(false);
  orderId = signal<string | null>(null);
  paymentId = signal<string | null>(null);
  stripeElements: StripeElements | null = null;
  
  checkoutForm = this.fb.group({
    orderType: ['Pickup', Validators.required],
    deliveryInstructions: ['']
  });

  orderTypes = [
    { label: 'Pickup', value: 'Pickup' },
    { label: 'Delivery', value: 'Delivery' }
  ];

  ngOnInit(): void {
    if (this.cartItems().length === 0) {
      this.router.navigate(['/menu']);
    }

    if (!this.authState.isLoggedIn()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Login Required',
        detail: 'Please login to place an order'
      });
      this.router.navigate(['/login']);
    }
  }

  ngAfterViewInit(): void {
    // Stripe elements initialized after order creation
  }

  async submitOrder(): Promise<void> {
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

    this.http.post<any>(`${API_BASE_URL}/orders`, orderRequest, { headers }).subscribe({
      next: async (response) => {
        const createdOrderId = response.orderId || response.id;
        this.orderId.set(createdOrderId);
        await this.initializePayment(createdOrderId);
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

  async initializePayment(orderId: string): Promise<void> {
    try {
      const stripe = await this.paymentService.initializeStripe();
      if (!stripe) {
        throw new Error('Failed to initialize Stripe');
      }

      this.paymentService.createPaymentIntent({ orderId }).subscribe({
        next: async (paymentResponse) => {
          this.paymentId.set(paymentResponse.paymentId);
          this.showPayment.set(true);
          this.isSubmitting.set(false);

          setTimeout(async () => {
            const elements = stripe.elements({
              clientSecret: paymentResponse.clientSecret,
            });

            const paymentElement = elements.create('payment');
            paymentElement.mount(this.paymentElementRef.nativeElement);

            this.stripeElements = elements;
          }, 100);
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Payment Error',
            detail: 'Failed to initialize payment'
          });
          this.isSubmitting.set(false);
        }
      });
    } catch (error) {
      this.messageService.add({
        severity: 'error',
        summary: 'Payment Error',
        detail: 'Failed to initialize Stripe'
      });
      this.isSubmitting.set(false);
    }
  }

  async completePayment(): Promise<void> {
    if (!this.stripeElements || !this.paymentId()) {
      return;
    }

    this.isSubmitting.set(true);

    try {
      const paymentIntent = await this.paymentService.processPayment(
        '',
        this.stripeElements
      );

      if (paymentIntent && paymentIntent.status === 'succeeded') {
        this.paymentService.confirmPayment({
          paymentId: this.paymentId()!,
          paymentIntentId: paymentIntent.id
        }).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Payment Successful!',
              detail: 'Your order has been placed and paid'
            });
            this.cartService.clearCart();
            this.router.navigate(['/orders']);
          },
          error: (err) => {
            this.messageService.add({
              severity: 'warn',
              summary: 'Payment Processed',
              detail: 'Payment succeeded but confirmation pending'
            });
            this.cartService.clearCart();
            this.router.navigate(['/orders']);
          }
        });
      }
    } catch (error: any) {
      this.messageService.add({
        severity: 'error',
        summary: 'Payment Failed',
        detail: error.message || 'Failed to process payment'
      });
      this.isSubmitting.set(false);
    }
  }

  goBack(): void {
    this.router.navigate(['/menu']);
  }
}
