import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { AccountSettings } from './account-settings';
import { AuthService } from '@core/services/auth.service';

describe('AccountSettings', () => {
  let component: AccountSettings;
  let fixture: ComponentFixture<AccountSettings>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccountSettings],
      providers: [
        {
          provide: AuthService,
          useValue: {
            loadCurrentUser: () => of({
              id: 'user-1',
              firstName: 'Test',
              lastName: 'User',
              email: 'test@example.com',
              roles: ['User'],
            }),
            getCurrentUser: () => ({
              id: 'user-1',
              firstName: 'Test',
              lastName: 'User',
              email: 'test@example.com',
              roles: ['User'],
            }),
            updateUser: () => of({ token: 'test-token' }),
            changePassword: () => of(void 0),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AccountSettings);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
