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
    const accountSelect = fixture.nativeElement.querySelectorAll('select')[1] as HTMLSelectElement;
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
