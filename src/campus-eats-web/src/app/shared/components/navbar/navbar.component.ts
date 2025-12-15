import { Component, inject, viewChild, ChangeDetectionStrategy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CartComponent } from '../cart/cart.component';
import { CartService } from '../../services/cart.service';
import { AuthStateService } from '../../services/auth-state.service';

@Component({
  selector: 'app-navbar',
  imports: [CommonModule, RouterLink, RouterLinkActive, ButtonModule, CartComponent],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NavbarComponent {
  cartService = inject(CartService);
  cartComponent = viewChild.required(CartComponent);
  authState = inject(AuthStateService);
  private router = inject(Router);

  showUserMenu = signal(false);

  openCart(): void {
    this.cartComponent().showCart();
  }

  toggleUserMenu(): void {
    this.showUserMenu.update((val) => !val);
  }

  logout(): void {
    this.authState.clearSession();
    this.showUserMenu.set(false);
    this.router.navigateByUrl('/login');
  }
}
