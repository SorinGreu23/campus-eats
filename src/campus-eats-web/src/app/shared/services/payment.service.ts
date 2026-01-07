import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthStateService } from './auth-state.service';
import { loadStripe, Stripe, StripeElements, PaymentIntent } from '@stripe/stripe-js';

const API_BASE_URL = 'http://localhost:5001/api';

export interface StripeConfig {
  publishableKey: string;
}

export interface CreatePaymentIntentRequest {
  orderId: string;
}

export interface CreatePaymentIntentResponse {
  clientSecret: string;
  paymentId: string;
  amount: number;
}

export interface ConfirmPaymentRequest {
  paymentId: string;
  paymentIntentId: string;
}

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private http = inject(HttpClient);
  private authState = inject(AuthStateService);
  private stripe: Stripe | null = null;

  async initializeStripe(): Promise<Stripe | null> {
    if (this.stripe) {
      return this.stripe;
    }

    const config = await this.getStripeConfig().toPromise();
    if (config) {
      this.stripe = await loadStripe(config.publishableKey);
    }
    return this.stripe;
  }

  getStripeConfig(): Observable<StripeConfig> {
    const headers = this.getAuthHeaders();
    return this.http.get<StripeConfig>(`${API_BASE_URL}/payments/config`, { headers });
  }

  createPaymentIntent(request: CreatePaymentIntentRequest): Observable<CreatePaymentIntentResponse> {
    const headers = this.getAuthHeaders();
    return this.http.post<CreatePaymentIntentResponse>(
      `${API_BASE_URL}/payments/create-payment-intent`,
      request,
      { headers }
    );
  }

  confirmPayment(request: ConfirmPaymentRequest): Observable<any> {
    const headers = this.getAuthHeaders();
    return this.http.post(
      `${API_BASE_URL}/payments/confirm`,
      request,
      { headers }
    );
  }

  async processPayment(clientSecret: string, elements: StripeElements): Promise<PaymentIntent | null> {
    if (!this.stripe) {
      throw new Error('Stripe not initialized');
    }

    const { error, paymentIntent } = await this.stripe.confirmPayment({
      elements,
      redirect: 'if_required',
    });

    if (error) {
      throw new Error(error.message || 'Payment failed');
    }

    return paymentIntent || null;
  }

  private getAuthHeaders(): HttpHeaders {
    const token = this.authState.token();
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }
}
