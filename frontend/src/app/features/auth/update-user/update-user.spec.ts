import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { UpdateUser } from './update-user';
import { AuthService } from '@core/services/auth.service';

describe('UpdateUser', () => {
  let component: UpdateUser;
  let fixture: ComponentFixture<UpdateUser>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UpdateUser],
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
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(UpdateUser);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
