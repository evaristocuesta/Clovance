import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
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
 * Guard for public routes (like login)
 * Redirects to home if user is already authenticated
 * Improves UX by preventing authenticated users from seeing login
 */
export const publicGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.isAuthenticated()
    ? router.createUrlTree(['/'])
    : true;
};