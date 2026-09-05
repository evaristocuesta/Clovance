import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PeriodNavigator } from './period-navigator';
import { SummaryPeriod } from '../models/summary-period.model';

describe('PeriodNavigator', () => {
  let fixture: ComponentFixture<PeriodNavigator>;
  let latestPeriod: SummaryPeriod | undefined;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PeriodNavigator],
    }).compileComponents();

    fixture = TestBed.createComponent(PeriodNavigator);
    fixture.componentRef.setInput('accounts', [
      { id: 'asset-account', name: 'Asset account', type: 'Checking', currency: 'EUR' },
      { id: 'liability-account', name: 'Loan', type: 'Loan', currency: 'EUR' },
    ]);
    fixture.componentRef.setInput('currencyOptions', [{ code: 'EUR', name: 'Euro', symbol: '€' }]);
    fixture.componentInstance.periodChange.subscribe((period) => latestPeriod = period);
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('clears a selected account when it is no longer available', async () => {
    // The currency select is only rendered when there is more than one currency,
    // so with a single currency the account select is the only/last one.
    const selects = fixture.nativeElement.querySelectorAll('select') as NodeListOf<HTMLSelectElement>;
    const accountSelect = selects[selects.length - 1];
    accountSelect.value = 'asset-account';
    accountSelect.dispatchEvent(new Event('change'));
    fixture.detectChanges();
    await fixture.whenStable();

    fixture.componentRef.setInput('accounts', [
      { id: 'liability-account', name: 'Loan', type: 'Loan', currency: 'EUR' },
    ]);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(latestPeriod?.accountId).toBeNull();
  });
});
