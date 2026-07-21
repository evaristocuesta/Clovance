import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { Transactions } from './transactions';
import { AccountService } from '@features/accounts/services/account.service';
import { TransactionService } from '../services/transaction.service';

describe('Transactions', () => {
  let component: Transactions;
  let fixture: ComponentFixture<Transactions>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Transactions],
      providers: [
        {
          provide: AccountService,
          useValue: {
            getCurrencies: () => of([]),
            getAccounts: () => of([]),
          },
        },
        {
          provide: TransactionService,
          useValue: {
            getTransactionsPage: () => of({ items: [], hasMore: false, nextCursorDate: null, nextCursorId: null }),
            getTransactionById: () => of({
              id: 'tx-2',
              accountId: 'acc-1',
              accountName: 'Main account',
              currency: 'EUR',
              amount: 10,
              date: new Date(),
              description: 'Related',
              type: 'Income',
            }),
            deleteTransaction: () => of(void 0),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Transactions);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
