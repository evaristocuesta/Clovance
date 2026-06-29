import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { map, Observable, tap } from 'rxjs';
import { LoginRequest, LoginResponse, RefreshResult, SetupRequest, TokenPayload, UserInfo, CreateInvitationCommand, CreateInvitationResult, RegisterWithInvitationRequest, RegisterWithInvitationResult, UpdateUserRequest, ChangePasswordRequest, UpdateUserResult } from '../models/auth.models';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http   = inject(HttpClient);

  // Access token lives only in memory — never in localStorage
  private _accessToken = signal<string | null>(null);
  private _currentUser = signal<UserInfo | null>(null);

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
            this.loadCurrentUser().subscribe(); // Load current user info after login
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
          next: response => {
            this._accessToken.set(response.token);
            this.loadCurrentUser().subscribe(); // Load current user info after token refresh
          }
        })
      );
  }

  logout() {
    return this.http
      .post<void>('/api/auth/logout', {}, { withCredentials: true })
      .pipe(
        tap({
          next: () => {
            this._accessToken.set(null);
            this._currentUser.set(null);
          }
        })
      );
  }

  setup(setupRequest: SetupRequest): Observable<void> {
    return this.http
      .post<void>('/api/auth/setup', setupRequest);
  }

  setupCompleted(): Observable<boolean> {
    return this.http
      .get<{ isSetupCompleted: boolean }>('/api/auth/setup-completed')
      .pipe(
        map(response => response.isSetupCompleted)
      );
  }

  getAccessToken(): string | null {
    return this._accessToken.asReadonly()();
  }

  getCurrentUser(): UserInfo | null {
    return this._currentUser.asReadonly()();
  }

  loadCurrentUser(): Observable<UserInfo> {
    return this.http
      .get<UserInfo>('/api/auth/users/me')
      .pipe(
        tap({
          next: user => this._currentUser.set(user)
        })
      );
  }

  getUsers(): Observable<UserInfo[]> {
    return this.http
      .get<{ users: UserInfo[] }>('/api/auth/users')
      .pipe(map(response => response.users));
  }

  deleteUser(id: string): Observable<void> {
    return this.http
      .delete<void>(`/api/auth/users/${id}`);
  }

  sendInvitation(userInvitation: CreateInvitationCommand): Observable<CreateInvitationResult> {
    return this.http
      .post<CreateInvitationResult>('/api/auth/invitations', userInvitation);
  }

  registerWithInvitation(request: RegisterWithInvitationRequest): Observable<RegisterWithInvitationResult> {
    return this.http
      .post<RegisterWithInvitationResult>('/api/auth/users/register', request);
  }

  updateUser(request: UpdateUserRequest): Observable<UpdateUserResult> {
    return this.http
      .put<UpdateUserResult>('/api/auth/users', request)
      .pipe(
        tap({
          next: (result) => {
            this._accessToken.set(result.token);
            this.loadCurrentUser().subscribe(); // Reload current user info after update
          }
        })
      );
  }

  changePassword(changePasswordRequest: ChangePasswordRequest): Observable<void> {
    return this.http
      .put<void>('/api/auth/users/password', changePasswordRequest);
  }

  readonly isAdmin = computed((): boolean => {
    const user = this.getCurrentUser();
    return user ? user.roles.includes('Admin') : false;
  });

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
