import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { AuthService } from '@core/services/auth.service';

/**
 * Guard for protected routes
 * Verifies if the user is authenticated using the signal
 * Does not make HTTP calls, it's synchronous and efficient
 */
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.isAuthenticated()
    ? true
    : router.createUrlTree(['/auth/login']);
};

/**
 * Guard for public routes (like login/setup)
 * - If authenticated: redirect to home
 * - If setup completed and accessing /auth/login: allow
 * - If setup completed and accessing /auth/setup: redirect to /auth/login
 * - If setup not completed and accessing /auth/login: redirect to /auth/setup
 * - If setup not completed and accessing /auth/setup: allow
 */
export const publicGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.isAuthenticated()
    ? router.createUrlTree(['/'])
    : authService.setupCompleted().pipe(
        map(isSetupCompleted => {
          const currentUrl = state.url;

          if (isSetupCompleted) {
            // Setup completed
            if (currentUrl.includes('/auth/login')) {
              return true; // Allow access to login
            } else if (currentUrl.includes('/auth/setup')) {
              return router.createUrlTree(['/auth/login']); // Redirect to login
            }
          } else {
            // Setup not completed
            if (currentUrl.includes('/auth/login')) {
              return router.createUrlTree(['/auth/setup']); // Redirect to setup
            } else if (currentUrl.includes('/auth/setup')) {
              return true; // Allow access to setup
            }
          }

          return true;
        })
      );
};