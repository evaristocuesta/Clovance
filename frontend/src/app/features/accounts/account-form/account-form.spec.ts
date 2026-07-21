import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { AccountForm } from './account-form';
import { AccountService } from '../services/account.service';

describe('AccountForm', () => {
  let component: AccountForm;
  let fixture: ComponentFixture<AccountForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccountForm],
      providers: [
        {
          provide: AccountService,
          useValue: {
            getCurrencies: () => of([]),
            getAccountById: () => of({ id: '', name: '', currency: '' }),
            updateAccount: () => of({}),
            createAccount: () => of({}),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AccountForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
