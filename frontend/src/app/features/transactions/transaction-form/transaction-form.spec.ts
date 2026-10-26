import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DIALOG_DATA } from '@angular/cdk/dialog';
import { of } from 'rxjs';
import { TransactionService } from '../services/transaction.service';
import { Transaction } from '../models/transaction.model';

import { TransactionForm } from './transaction-form';

describe('TransactionForm', () => {
  let component: TransactionForm;
  let fixture: ComponentFixture<TransactionForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TransactionForm],
      providers: [{ provide: DIALOG_DATA, useValue: { type: 'Income', accounts: [] } }],
    }).compileComponents();

    fixture = TestBed.createComponent(TransactionForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

describe('TransactionForm editing a transaction changed elsewhere', () => {
  let component: TransactionForm;
  let fixture: ComponentFixture<TransactionForm>;

  // What the in-memory list still holds, and what the API has since it was last saved
  // elsewhere. Editing from the stale copy is what produced the permanent 409.
  const staleTransaction: Transaction = {
    id: 'tx-1',
    accountId: 'acc-1',
    accountName: 'Main account',
    currency: 'EUR',
    amount: 100,
    date: new Date('2026-01-15'),
    description: 'Salary',
    type: 'Income',
    version: 1,
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TransactionForm],
      providers: [
        {
          provide: DIALOG_DATA,
          useValue: { transaction: staleTransaction, type: 'Income', accounts: [] },
        },
        {
          provide: TransactionService,
          useValue: {
            getTransactionById: () => of({ ...staleTransaction, description: 'Salary updated', version: 7 }),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(TransactionForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('re-reads the transaction so the save is not based on a stale version', () => {
    expect(component.transaction().version).toBe(7);
    expect(component.transaction().description).toBe('Salary updated');
  });
});
