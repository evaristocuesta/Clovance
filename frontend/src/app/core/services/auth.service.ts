import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { LoginRequest, LoginResponse, RefreshResult, TokenPayload } from '../models/auth.models';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http   = inject(HttpClient);

  // Access token lives only in memory — never in localStorage
  private _accessToken = signal<string | null>(null);

  readonly isAuthenticated = computed(() => {
    const token = this._accessToken();
    if (!token) return false;
    return !this.isTokenExpired(token);
  });

  readonly currentUser = computed(() => {
    const token = this._accessToken();
    return token ? this.decodeToken(token) : null;
  });

  login(credentials: LoginRequest) : Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>('/api/auth/login', credentials, {
        withCredentials: true, //  receives the HttpOnly cookie with the refresh token
      })
      .pipe(
        tap({
          next: response => {
            this._accessToken.set(response.access_token);
          }
        })
      );
  }

  refreshToken() {
    return this.http
      .post<RefreshResult>('/api/auth/refresh', {}, {
        withCredentials: true,
      })
      .pipe(
        tap({
          next: response => this._accessToken.set(response.token)
        })
      );
  }

  logout() {
    return this.http
      .post<void>('/api/auth/logout', {}, { withCredentials: true })
      .pipe(
        tap({
          next: () => this._accessToken.set(null)
        })
      );
  }

  getAccessToken(): string | null {
    return this._accessToken();
  }

  private decodeToken(token: string): TokenPayload | null {
    try {
      const payload = token.split('.')[1];
      return JSON.parse(atob(payload)) as TokenPayload;
    } catch {
      return null;
    }
  }

  private isTokenExpired(token: string): boolean {
    const payload = this.decodeToken(token);
    if (!payload) return true;
    return Date.now() >= payload.exp * 1000;
  }
}