import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, CanActivateFn, RedirectCommand, Router, RouterStateSnapshot, provideRouter, UrlTree } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import { firstValueFrom, isObservable, Observable, of, throwError } from 'rxjs';

import { adminGuard, authGuard } from './auth.guard';

describe('authGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) =>
    TestBed.runInInjectionContext(() => authGuard(...guardParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeGuard).toBeTruthy();
  });
});

describe('adminGuard', () => {
  const adminUser = {
    id: '01900000-0000-7000-8000-000000000001',
    firstName: 'Admin',
    lastName: 'User',
    email: 'admin@example.com',
    roles: ['Admin'],
  };

  const regularUser = {
    id: '01900000-0000-7000-8000-000000000002',
    firstName: 'Regular',
    lastName: 'User',
    email: 'user@example.com',
    roles: ['User'],
  };

  type GuardOutcome = boolean | UrlTree | RedirectCommand;
  type GuardReturn = GuardOutcome | Promise<GuardOutcome> | Observable<GuardOutcome>;

  let authStub: Partial<AuthService>;

  const executeGuard = (): GuardReturn =>
    TestBed.runInInjectionContext(() =>
      adminGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot)
    );

  const settle = async (result: GuardReturn): Promise<GuardOutcome> => {
    if (isObservable(result)) {
      return await firstValueFrom(result);
    }

    return await result;
  };

  const homeUrlTree = () => TestBed.inject(Router).createUrlTree(['/']);

  beforeEach(() => {
    authStub = {
      getCurrentUser: () => null,
      loadCurrentUser: () => of(adminUser),
    };

    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: AuthService, useFactory: () => authStub as unknown as AuthService },
      ],
    });
  });

  it('allows an admin user', async () => {
    authStub.getCurrentUser = () => adminUser;

    const result = await settle(executeGuard());

    expect(result).toBe(true);
  });

  it('redirects a non-admin user to the home route', async () => {
    authStub.getCurrentUser = () => regularUser;

    const result = await settle(executeGuard());

    expect(result).toEqual(homeUrlTree());
  });

  it('loads the current user when it is not loaded yet and allows an admin', async () => {
    const result = await settle(executeGuard());

    expect(result).toBe(true);
  });

  it('redirects to the home route when the loaded user is not an admin', async () => {
    authStub.loadCurrentUser = () => of(regularUser);

    const result = await settle(executeGuard());

    expect(result).toEqual(homeUrlTree());
  });

  it('redirects to the home route when the current user cannot be loaded', async () => {
    authStub.loadCurrentUser = () => throwError(() => new Error('load failed'));

    const result = await settle(executeGuard());

    expect(result).toEqual(homeUrlTree());
  });
});
