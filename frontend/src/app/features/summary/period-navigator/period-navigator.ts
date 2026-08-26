import { ChangeDetectionStrategy, Component, computed, effect, input, output, signal } from '@angular/core';
import { TranslocoDirective } from '@jsverse/transloco';
import { Account } from '@features/accounts/models/account.model';
import { Currency } from '@features/accounts/models/currency.model';
import { Icon } from '@shared/ui/icon/icon';
import { SummaryPeriod } from '../models/summary-period.model';
import { getCurrencySymbol } from '@shared/utils/currency-utils';

@Component({
  selector: 'app-period-navigator',
  imports: [TranslocoDirective, Icon],
  templateUrl: './period-navigator.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './period-navigator.css',
})
export class PeriodNavigator {
  private static readonly CURRENCY_KEY = 'summary-currency';

  readonly accounts = input<Account[]>([]);
  readonly currencyOptions = input<Currency[]>([]);
  readonly periodChange = output<SummaryPeriod>();

  private readonly today = new Date();
  protected readonly year = signal(this.today.getFullYear());
  protected readonly month = signal(this.today.getMonth() + 1);
  protected readonly viewMode = signal<'monthly' | 'daily'>('monthly');
  protected readonly accountId = signal<string | null>(null);
  protected readonly currency = signal(this.getStoredCurrency());

  protected readonly activeAccounts = computed(() => this.accounts().filter((account) => !account.isDeleted));
  protected readonly currencies = computed(() => {
    const accountCurrencies = new Set(this.activeAccounts().map((account) => account.currency.toUpperCase()));
    const catalogCurrencies = this.currencyOptions()
      .map((currency) => currency.code.toUpperCase())
      .filter((currency) => accountCurrencies.has(currency));
    const accountCurrencyCodes = Array.from(accountCurrencies).filter((currency) => !catalogCurrencies.includes(currency));

    return [...catalogCurrencies, ...accountCurrencyCodes];
  });

  protected readonly currencySymbol = (code: string): string =>
    getCurrencySymbol(code, this.currencyOptions());

  protected readonly periodLabel = computed(() => {
    const date = new Date(this.year(), this.month() - 1, 1);
    return date.toLocaleDateString(undefined, { month: 'long', year: 'numeric' });
  });

  constructor() {
    effect(() => {
      const availableCurrencies = this.currencies();
      const firstAvailableCurrency = availableCurrencies[0];
      if (firstAvailableCurrency && !availableCurrencies.includes(this.currency())) {
        this.currency.set(firstAvailableCurrency);
        globalThis.localStorage?.setItem(PeriodNavigator.CURRENCY_KEY, firstAvailableCurrency);
        return;
      }

      this.periodChange.emit({
        year: this.year(),
        month: this.month(),
        viewMode: this.viewMode(),
        accountId: this.accountId(),
        currency: this.currency(),
      });
    });
  }

  protected previousMonth(): void {
    this.shiftMonth(-1);
  }

  protected nextMonth(): void {
    this.shiftMonth(1);
  }

  protected setViewMode(mode: 'monthly' | 'daily'): void {
    this.viewMode.set(mode);
  }

  protected onAccountChange(value: string): void {
    this.accountId.set(value || null);
  }

  protected onCurrencyChange(value: string): void {
    const currency = value.toUpperCase();
    this.currency.set(currency);
    globalThis.localStorage?.setItem(PeriodNavigator.CURRENCY_KEY, currency);
  }

  private shiftMonth(delta: number): void {
    const date = new Date(this.year(), this.month() - 1 + delta, 1);
    this.year.set(date.getFullYear());
    this.month.set(date.getMonth() + 1);
  }

  private getStoredCurrency(): string {
    return globalThis.localStorage?.getItem(PeriodNavigator.CURRENCY_KEY)?.toUpperCase() || 'EUR';
  }

}
