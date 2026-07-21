import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { Accounts } from './accounts';
import { AccountService } from '../services/account.service';

describe('Accounts', () => {
  let component: Accounts;
  let fixture: ComponentFixture<Accounts>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Accounts],
      providers: [
        {
          provide: AccountService,
          useValue: {
            getCurrencies: () => of([]),
            getAccounts: () => of([]),
            deleteAccount: () => of(void 0),
            restoreAccount: () => of(void 0),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Accounts);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
