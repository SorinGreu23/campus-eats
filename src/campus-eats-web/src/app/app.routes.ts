import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'menu',
    pathMatch: 'full'
  },
  {
    path: 'menu',
    loadComponent: () => import('./features/menu/components/menu-list/menu-list.component').then(m => m.MenuListComponent)
  },
  {
    path: 'orders',
    loadComponent: () => import('./features/orders/components/order-list/order-list.component').then(m => m.OrderListComponent)
  },
  {
    path: 'loyalty',
    loadComponent: () => import('./features/loyalty/components/loyalty-dashboard/loyalty-dashboard.component').then(m => m.LoyaltyDashboardComponent)
  },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/components/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/components/register/register.component').then(m => m.RegisterComponent)
  }
];
