import { Injectable, computed, signal } from '@angular/core';

const TOKEN_KEY = 'jwt';
const NAME_KEY = 'userDisplayName';
const ROLE_KEY = 'userRole';

@Injectable({ providedIn: 'root' })
export class AuthStateService {
  private displayNameSignal = signal<string | null>(localStorage.getItem(NAME_KEY));
  private tokenSignal = signal<string | null>(localStorage.getItem(TOKEN_KEY));
  private roleSignal = signal<string | null>(localStorage.getItem(ROLE_KEY));

  isLoggedIn = computed(() => !!this.tokenSignal());
  role = this.roleSignal.asReadonly();
  isKitchen = computed(() => this.roleSignal() === 'Kitchen');
  displayName = this.displayNameSignal.asReadonly();
  token = this.tokenSignal.asReadonly();

  setSession(displayName: string, role: string, token: string): void {
    const name = displayName.trim();
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(NAME_KEY, name);
    localStorage.setItem(ROLE_KEY, role);
    this.tokenSignal.set(token);
    this.displayNameSignal.set(name || 'User');
    this.roleSignal.set(role);
  }

  clearSession(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(NAME_KEY);
    localStorage.removeItem(ROLE_KEY);
    this.tokenSignal.set(null);
    this.displayNameSignal.set(null);
    this.roleSignal.set(null);
  }
}
