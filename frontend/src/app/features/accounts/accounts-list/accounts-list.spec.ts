import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { AccountsList } from './accounts-list';
import { AccountService } from '../services/account.service';

describe('AccountsList', () => {
  let component: AccountsList;
  let fixture: ComponentFixture<AccountsList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccountsList],
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

    fixture = TestBed.createComponent(AccountsList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
