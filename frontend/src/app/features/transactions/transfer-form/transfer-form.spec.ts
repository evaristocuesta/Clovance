import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DIALOG_DATA } from '@angular/cdk/dialog';
import { of } from 'rxjs';
import { TransactionService } from '../services/transaction.service';
import { Transaction } from '../models/transaction.model';

import { TransferForm } from './transfer-form';

describe('TransferForm', () => {
  let component: TransferForm;
  let fixture: ComponentFixture<TransferForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TransferForm],
      providers: [{ provide: DIALOG_DATA, useValue: { accounts: [] } }],
    }).compileComponents();

    fixture = TestBed.createComponent(TransferForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

describe('TransferForm editing a transfer changed elsewhere', () => {
  let component: TransferForm;
  let fixture: ComponentFixture<TransferForm>;

  // What the in-memory list still holds, and what the API has since. The dialog is
  // addressed by the positive leg, so that is the one carrying "version".
  const staleFromTransaction: Transaction = {
    id: 'tx-from',
    accountId: 'acc-1',
    accountName: 'Checking',
    currency: 'EUR',
    amount: -150,
    date: new Date('2026-01-15'),
    description: 'Move funds',
    type: 'Transfer',
    relatedTransactionId: 'tx-to',
    version: 1,
  };

  const staleToTransaction: Transaction = {
    id: 'tx-to',
    accountId: 'acc-2',
    accountName: 'Savings',
    currency: 'EUR',
    amount: 150,
    date: new Date('2026-01-15'),
    description: 'Move funds',
    type: 'Transfer',
    relatedTransactionId: 'tx-from',
    version: 2,
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TransferForm],
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
                ? { ...staleToTransaction, description: 'Move funds updated', version: 7 }
                : { ...staleFromTransaction, description: 'Move funds updated', version: 8 }),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(TransferForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('re-reads both legs so the save is not based on stale versions', () => {
    expect(component.transferLegs()?.toTransaction.version).toBe(7);
    expect(component.transferLegs()?.fromTransaction.version).toBe(8);
  });
});
