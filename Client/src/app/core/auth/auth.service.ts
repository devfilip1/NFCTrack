import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { AuthResponse, LoginRequest, RegisterRequest } from './auth.models';

const STORAGE_KEY = 'shortlink.auth';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly session = signal<AuthResponse | null>(this.restore());

  readonly token = computed(() => this.session()?.accessToken ?? null);
  readonly isAuthenticated = computed(() => this.token() !== null);

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>('/api/auth/login', request)
      .pipe(tap(response => this.store(response)));
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>('/api/auth/register', request)
      .pipe(tap(response => this.store(response)));
  }

  logout(): void {
    localStorage.removeItem(STORAGE_KEY);
    this.session.set(null);
  }

  private store(response: AuthResponse): void {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(response));
    this.session.set(response);
  }

  private restore(): AuthResponse | null {
    const raw = localStorage.getItem(STORAGE_KEY);

    if (!raw) {
      return null;
    }

    const saved = JSON.parse(raw) as AuthResponse;

    return new Date(saved.expiresAt) > new Date() ? saved : null;
  }
}
