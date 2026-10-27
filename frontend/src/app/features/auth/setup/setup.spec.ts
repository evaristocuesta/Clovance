import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Setup } from './setup';

describe('Setup', () => {
  let component: Setup;
  let fixture: ComponentFixture<Setup>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Setup],
    }).compileComponents();

    fixture = TestBed.createComponent(Setup);
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
