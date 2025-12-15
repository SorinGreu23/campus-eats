import { inject } from '@angular/core';
import { CanMatchFn, Router } from '@angular/router';
import { AuthStateService } from '../services/auth-state.service';

export const nonKitchenGuard: CanMatchFn = () => {
  const auth = inject(AuthStateService);
  const router = inject(Router);

  if (!auth.isLoggedIn()) {
    router.navigateByUrl('/login');
    return false;
  }

  if (!auth.isKitchen()) return true;

  router.navigateByUrl('/kitchen');
  return false;
};
