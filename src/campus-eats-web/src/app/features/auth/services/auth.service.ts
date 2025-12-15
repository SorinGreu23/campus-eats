import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface LoginResponse {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  token: string;
}

export interface RegisterPayload {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  role: string;
  userName: string;
}

export interface RegisterResponse {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  userName: string;
  role: string;
}

// TODO: move to environment configuration when available
const API_BASE_URL = 'http://localhost:5001/api';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);

  login(credentials: LoginCredentials): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${API_BASE_URL}/users/login`, credentials);
  }

  register(payload: RegisterPayload): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(`${API_BASE_URL}/users/register`, payload);
  }
}
