import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AccountCard } from './account-card';

describe('AccountCard', () => {
  let component: AccountCard;
  let fixture: ComponentFixture<AccountCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccountCard],
    }).compileComponents();

    fixture = TestBed.createComponent(AccountCard);
    fixture.componentRef.setInput('account', {
      id: 'acc-1',
      name: 'Main account',
      currency: 'EUR',
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
