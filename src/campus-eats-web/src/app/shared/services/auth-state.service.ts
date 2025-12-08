import { Injectable, computed, signal } from '@angular/core';

const TOKEN_KEY = 'jwt';
const NAME_KEY = 'userDisplayName';

@Injectable({ providedIn: 'root' })
export class AuthStateService {
  private displayNameSignal = signal<string | null>(localStorage.getItem(NAME_KEY));
  private tokenSignal = signal<string | null>(localStorage.getItem(TOKEN_KEY));

  isLoggedIn = computed(() => !!this.tokenSignal());
  displayName = this.displayNameSignal.asReadonly();
  token = this.tokenSignal.asReadonly();

  setSession(displayName: string, token: string): void {
    const name = displayName.trim();
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(NAME_KEY, name);
    this.tokenSignal.set(token);
    this.displayNameSignal.set(name || 'User');
  }

  clearSession(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(NAME_KEY);
    this.tokenSignal.set(null);
    this.displayNameSignal.set(null);
  }
}
