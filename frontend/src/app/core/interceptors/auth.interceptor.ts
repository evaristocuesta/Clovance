import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { EMPTY, Observable, catchError, finalize, shareReplay, switchMap, throwError } from 'rxjs';
import { AuthService } from '@core/services/auth.service';
import { RefreshResult } from '@core/models/auth.models';

let refreshInProgress$: Observable<RefreshResult> | null = null;
let logoutInProgress$: Observable<void> | null = null;

function refreshTokenSingleFlight(auth: AuthService): Observable<RefreshResult> {
  if (!refreshInProgress$) {
    refreshInProgress$ = auth.refreshToken().pipe(
      catchError((error: unknown) => {
        logoutSingleFlight(auth).subscribe();
        return throwError(() => error);
      }),
      finalize(() => {
        refreshInProgress$ = null;
      }),
      shareReplay({ bufferSize: 1, refCount: false }),
    );
  }

  return refreshInProgress$;
}

function logoutSingleFlight(auth: AuthService): Observable<void> {
  if (!logoutInProgress$) {
    logoutInProgress$ = auth.logout().pipe(
      catchError(() => EMPTY),
      finalize(() => {
        auth.clearSession();
        logoutInProgress$ = null;
      }),
      shareReplay({ bufferSize: 1, refCount: false }),
    );
  }

  return logoutInProgress$;
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const isApiCall = req.url.startsWith('/api/');

  if (!isApiCall) {
    return next(req);
  }

  const auth = inject(AuthService);
  const token = auth.getAccessToken();

  const authCalls = ['/auth/login', '/auth/refresh', '/auth/logout', '/auth/users/register'];

  const authReq = token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !authCalls.some((call) => req.url.includes(call))) {
        // Share one refresh among concurrent requests that receive 401 together.
        return refreshTokenSingleFlight(auth).pipe(
          switchMap((response) => {
            const retried = req.clone({
              setHeaders: { Authorization: `Bearer ${response.token}` },
            });
            return next(retried).pipe(
              catchError((retryError: unknown) => {
                logoutSingleFlight(auth).subscribe();
                return throwError(() => retryError);
              }),
            );
          }),
          catchError(() => {
            return throwError(() => error);
          }),
        );
      }
      return throwError(() => error);
    }),
  );
};
