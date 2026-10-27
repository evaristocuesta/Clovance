import { HttpErrorResponse, HttpHandlerFn, HttpRequest, HttpResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { HttpInterceptorFn } from '@angular/common/http';
import { Subject, of, throwError } from 'rxjs';
import { RefreshResult } from '@core/models/auth.models';
import { AuthService } from '@core/services/auth.service';

import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  const interceptor: HttpInterceptorFn = (req, next) =>
    TestBed.runInInjectionContext(() => authInterceptor(req, next));

  let refreshResult$: Subject<RefreshResult>;
  let refreshCalls: number;
  let logoutCalls: number;
  let clearSessionCalls: number;

  beforeEach(() => {
    refreshResult$ = new Subject<RefreshResult>();
    refreshCalls = 0;
    logoutCalls = 0;
    clearSessionCalls = 0;

    TestBed.configureTestingModule({
      providers: [
        {
          provide: AuthService,
          useValue: {
            getAccessToken: () => 'expired-token',
            refreshToken: () => {
              refreshCalls++;
              return refreshResult$.asObservable();
            },
            logout: () => {
              logoutCalls++;
              return throwError(() => new HttpErrorResponse({ status: 401 }));
            },
            clearSession: () => clearSessionCalls++,
          },
        },
      ],
    });
  });

  it('should be created', () => {
    expect(interceptor).toBeTruthy();
  });

  it('shares one refresh between requests that receive 401 concurrently', () => {
    const retriedRequests: HttpRequest<unknown>[] = [];
    const completedRequests: string[] = [];
    const errors: unknown[] = [];
    const next: HttpHandlerFn = (request) => {
      if (request.headers.get('Authorization') === 'Bearer fresh-token') {
        retriedRequests.push(request);
        return of(new HttpResponse({ status: 200 }));
      }

      return throwError(() => new HttpErrorResponse({ status: 401, url: request.url }));
    };

    interceptor(new HttpRequest('GET', '/api/transactions?page=1'), next).subscribe({
      next: () => completedRequests.push('first'),
      error: (error) => errors.push(error),
    });
    interceptor(new HttpRequest('GET', '/api/transactions?page=2'), next).subscribe({
      next: () => completedRequests.push('second'),
      error: (error) => errors.push(error),
    });

    expect(refreshCalls).toBe(1);

    refreshResult$.next({ token: 'fresh-token', expiresAt: new Date() });
    refreshResult$.complete();

    expect(retriedRequests.map((request) => request.url)).toEqual([
      '/api/transactions?page=1',
      '/api/transactions?page=2',
    ]);
    expect(completedRequests).toEqual(['first', 'second']);
    expect(errors).toEqual([]);
    expect(logoutCalls).toBe(0);
  });

  it('logs out once when a shared refresh fails', () => {
    const errors: unknown[] = [];
    const next: HttpHandlerFn = (request) =>
      throwError(() => new HttpErrorResponse({ status: 401, url: request.url }));

    interceptor(new HttpRequest('GET', '/api/transactions?page=1'), next).subscribe({
      error: (error) => errors.push(error),
    });
    interceptor(new HttpRequest('GET', '/api/transactions?page=2'), next).subscribe({
      error: (error) => errors.push(error),
    });

    expect(refreshCalls).toBe(1);

    refreshResult$.error(new HttpErrorResponse({ status: 401, url: '/api/auth/refresh' }));

    expect(errors).toHaveLength(2);
    expect(logoutCalls).toBe(1);
    expect(clearSessionCalls).toBe(1);
  });

  it('does not try to refresh the logout request itself', () => {
    const errors: unknown[] = [];
    const next: HttpHandlerFn = (request) =>
      throwError(() => new HttpErrorResponse({ status: 401, url: request.url }));

    interceptor(new HttpRequest('POST', '/api/auth/logout', {}), next).subscribe({
      error: (error) => errors.push(error),
    });

    expect(errors).toHaveLength(1);
    expect(refreshCalls).toBe(0);
  });
});
