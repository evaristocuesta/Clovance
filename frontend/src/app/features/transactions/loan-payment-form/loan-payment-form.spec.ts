import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DIALOG_DATA } from '@angular/cdk/dialog';
import { of } from 'rxjs';
import { TransactionService } from '../services/transaction.service';
import { Transaction } from '../models/transaction.model';

import { LoanPaymentForm } from './loan-payment-form';

describe('LoanPaymentForm', () => {
  let component: LoanPaymentForm;
  let fixture: ComponentFixture<LoanPaymentForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoanPaymentForm],
      providers: [{ provide: DIALOG_DATA, useValue: { accounts: [] } }],
    }).compileComponents();

    fixture = TestBed.createComponent(LoanPaymentForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

describe('LoanPaymentForm editing a loan payment changed elsewhere', () => {
  let component: LoanPaymentForm;
  let fixture: ComponentFixture<LoanPaymentForm>;

  // What the in-memory list still holds, and what the API has since. The list stores
  // the amounts on the negative leg, so that is the one the form reads them from.
  const staleFromTransaction: Transaction = {
    id: 'tx-from',
    accountId: 'acc-1',
    accountName: 'Checking',
    currency: 'EUR',
    amount: -400,
    principalAmount: -150,
    date: new Date('2026-01-15'),
    description: 'Loan instalment',
    type: 'LoanPayment',
    relatedTransactionId: 'tx-to',
    version: 1,
  };

  const staleToTransaction: Transaction = {
    id: 'tx-to',
    accountId: 'acc-2',
    accountName: 'Car loan',
    currency: 'EUR',
    amount: 400,
    date: new Date('2026-01-15'),
    description: 'Loan instalment',
    type: 'LoanPayment',
    relatedTransactionId: 'tx-from',
    version: 2,
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoanPaymentForm],
      providers: [
        {
          provide: DIALOG_DATA,
          useValue: { fromTransaction: staleFromTransaction, toTransaction: staleToTransaction, accounts: [] },
        },
        {
          provide: TransactionService,
          useValue: {
            getTransactionById: (id: string) =>
              of(id === staleToTransaction.id
                ? { ...staleToTransaction, description: 'Loan instalment updated', version: 7 }
                : { ...staleFromTransaction, description: 'Loan instalment updated', version: 8 }),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LoanPaymentForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('re-reads both legs so the save is not based on stale versions', () => {
    expect(component.loanPaymentLegs()?.toTransaction.version).toBe(7);
    expect(component.loanPaymentLegs()?.fromTransaction.version).toBe(8);
  });
});
