import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '@core/services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const isApiCall = req.url.startsWith('/api/');

  if (!isApiCall) {
    return next(req);
  }
  
  const auth = inject(AuthService);
  const token = auth.getAccessToken();

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !req.url.includes('/auth/')) {
        // Expired token → try refresh and retry the original request
        return auth.refreshToken().pipe(
          switchMap(response => {
            const retried = req.clone({
              setHeaders: { Authorization: `Bearer ${response.token}` },
            });
            return next(retried);
          }),
          catchError(() => {
            auth.logout().subscribe();
            return throwError(() => error);
          })
        );
      }
      return throwError(() => error);
    })
  );
};
