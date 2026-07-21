import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TransactionCard } from './transaction-card';

describe('TransactionCard', () => {
  let component: TransactionCard;
  let fixture: ComponentFixture<TransactionCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TransactionCard],
    }).compileComponents();

    fixture = TestBed.createComponent(TransactionCard);
    fixture.componentRef.setInput('transaction', {
      id: 'tx-1',
      accountId: 'acc-1',
      accountName: 'Main account',
      currency: 'EUR',
      amount: 100,
      date: new Date(),
      description: 'Test transaction',
      type: 'Income',
    });
    fixture.componentRef.setInput('currencySymbolMap', { EUR: '€' });
    fixture.detectChanges();
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
