import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';

import { RegisterUser } from './register-user';

describe('RegisterUser', () => {
  let component: RegisterUser;
  let fixture: ComponentFixture<RegisterUser>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RegisterUser],
      providers: [
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              queryParamMap: {
                get: (name: string) => name === 'email' ? 'new-user@example.com' : name === 'token' ? 'invite-token' : null,
              },
            },
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterUser);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('marks both password fields as new passwords for password managers', () => {
    const passwordInputs: NodeListOf<HTMLInputElement> = fixture.nativeElement.querySelectorAll('input[type="password"]');

    expect(passwordInputs.length).toBe(2);
    expect(Array.from(passwordInputs).every((input) => input.autocomplete === 'new-password')).toBe(true);
  });
});
