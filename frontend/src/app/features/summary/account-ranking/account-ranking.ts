import { ChangeDetectionStrategy, Component, computed, input, signal } from '@angular/core';
import { TranslocoDirective } from '@jsverse/transloco';
import { Account } from '@features/accounts/models/account.model';
import { Currency } from '@features/accounts/models/currency.model';
import { EChart, EChartsOption } from '@shared/ui/echart/echart';
import { AccountBalancePoint, AccountCashflowBreakdown } from '../models/summary.model';
import { formatCurrency } from '@shared/utils/currency-utils';

@Component({
  selector: 'app-account-ranking',
  imports: [EChart, TranslocoDirective],
  templateUrl: './account-ranking.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './account-ranking.css',
})
export class AccountRanking {
  readonly balanceByAccount = input<AccountBalancePoint[] | null>(null);
  readonly cashflowByAccount = input<AccountCashflowBreakdown[] | null>(null);
  readonly accounts = input<Account[]>([]);
  readonly currency = input('EUR');
  readonly currencyOptions = input<Currency[]>([]);
  readonly language = input('en');

  protected readonly metric = signal<'balance' | 'cashflow'>('balance');

  private readonly accountNameById = computed(() =>
    Object.fromEntries(this.accounts().map((account) => [account.id, account.name])),
  );

  protected readonly options = computed<EChartsOption>(() => {
    const nameById = this.accountNameById();

    const entries =
      this.metric() === 'balance'
        ? (this.balanceByAccount() ?? [])
            .map((entry) => ({ name: nameById[entry.accountId] ?? entry.accountId, value: entry.balance }))
            .filter((entry) => entry.value > 0)
            .sort((a, b) => b.value - a.value)
        : (this.cashflowByAccount() ?? [])
            .map((entry) => ({ name: nameById[entry.accountId] ?? entry.accountId, value: Math.abs(entry.expenses) }))
            .filter((entry) => entry.value > 0)
            .sort((a, b) => b.value - a.value);

    return {
      tooltip: {
        trigger: 'axis',
        axisPointer: { type: 'shadow' },
        valueFormatter: (value) => formatCurrency(Number(value), this.currency(), this.currencyOptions(), this.language()),
      },
      grid: { left: 120, right: 24, top: 16, bottom: 16 },
      xAxis: {
        type: 'value',
        axisLabel: { formatter: (value) => formatCurrency(Number(value), this.currency(), this.currencyOptions(), this.language()) },
      },
      yAxis: { type: 'category', data: entries.map((entry) => entry.name).reverse(), inverse: false },
      series: [
        {
          type: 'bar',
          itemStyle: { color: '#2563eb' },
          data: entries.map((entry) => entry.value).reverse(),
        },
      ],
    };
  });

  protected setMetric(metric: 'balance' | 'cashflow'): void {
    this.metric.set(metric);
  }
}
