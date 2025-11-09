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
  }
];
