import { Injectable, computed, signal } from '@angular/core';

const TOKEN_KEY = 'jwt';
const NAME_KEY = 'userDisplayName';
const ROLE_KEY = 'userRole';
const USER_ID_KEY = 'userId';

@Injectable({ providedIn: 'root' })
export class AuthStateService {
  private displayNameSignal = signal<string | null>(localStorage.getItem(NAME_KEY));
  private tokenSignal = signal<string | null>(localStorage.getItem(TOKEN_KEY));
  private roleSignal = signal<string | null>(localStorage.getItem(ROLE_KEY));
  private userIdSignal = signal<string | null>(localStorage.getItem(USER_ID_KEY));

  isLoggedIn = computed(() => !!this.tokenSignal());
  role = this.roleSignal.asReadonly();
  isKitchen = computed(() => this.roleSignal() === 'Kitchen');
  displayName = this.displayNameSignal.asReadonly();
  token = this.tokenSignal.asReadonly();
  userId = this.userIdSignal.asReadonly();

  setSession(displayName: string, role: string, token: string, userId?: string): void {
    const name = displayName.trim();
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(NAME_KEY, name);
    localStorage.setItem(ROLE_KEY, role);
    if (userId) {
      localStorage.setItem(USER_ID_KEY, userId);
      this.userIdSignal.set(userId);
    }
    this.tokenSignal.set(token);
    this.displayNameSignal.set(name || 'User');
    this.roleSignal.set(role);
  }

  clearSession(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(NAME_KEY);
    localStorage.removeItem(ROLE_KEY);
    localStorage.removeItem(USER_ID_KEY);
    this.tokenSignal.set(null);
    this.displayNameSignal.set(null);
    this.roleSignal.set(null);
    this.userIdSignal.set(null);
  }
}
