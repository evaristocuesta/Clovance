import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { combineLatest, distinctUntilChanged, switchMap } from 'rxjs';
import { TranslocoDirective, TranslocoService } from '@jsverse/transloco';
import { AccountService } from '@features/accounts/services/account.service';
import { Currency } from '@features/accounts/models/currency.model';
import { SummaryService } from '../services/summary.service';
import { PeriodNavigator } from '../period-navigator/period-navigator';
import { KpiCards } from '../kpi-cards/kpi-cards';
import { BalanceChart } from '../balance-chart/balance-chart';
import { CashflowChart } from '../cashflow-chart/cashflow-chart';
import { AccountRanking } from '../account-ranking/account-ranking';
import { SummaryPeriod } from '../models/summary-period.model';
import { BalanceChartPoint, CashflowChartPoint } from '../models/summary-chart.model';

@Component({
  selector: 'app-summary',
  imports: [TranslocoDirective, PeriodNavigator, KpiCards, BalanceChart, CashflowChart, AccountRanking],
  templateUrl: './summary.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './summary.css',
})
export class Summary {
  private static readonly CURRENCY_KEY = 'summary-currency';

  private readonly accountService = inject(AccountService);
  private readonly summaryService = inject(SummaryService);
  private readonly translocoService = inject(TranslocoService);

  protected readonly accounts = toSignal(this.accountService.getAccounts(), { initialValue: [] });
  protected readonly currencies = toSignal(this.accountService.getCurrencies(), { initialValue: [] as Currency[] });
  protected readonly language = toSignal(this.translocoService.langChanges$, {
    initialValue: this.translocoService.getActiveLang(),
  });

  private readonly today = new Date();
  protected readonly period = signal<SummaryPeriod>({
    year: this.today.getFullYear(),
    month: this.today.getMonth() + 1,
    viewMode: 'monthly',
    accountId: null,
    currency: this.getStoredCurrency(),
  });

  // PeriodNavigator emits its initial value on init too, so dedupe to avoid a duplicate first fetch
  private readonly period$ = toObservable(this.period).pipe(
    distinctUntilChanged(
      (a, b) => a.year === b.year && a.month === b.month && a.viewMode === b.viewMode && a.accountId === b.accountId && a.currency === b.currency,
    ),
  );

  // Monthly data always fetched: feeds KPIs, ranking, and the charts in monthly view
  private readonly monthlyData = toSignal(
    this.period$.pipe(
      switchMap((period) =>
        combineLatest([
          this.summaryService.getMonthlyBalance({ accountId: period.accountId ?? undefined, month: period.month, year: period.year, currency: period.currency }),
          this.summaryService.getMonthlyCashflow({ accountId: period.accountId ?? undefined, month: period.month, year: period.year, currency: period.currency }),
        ]),
      ),
    ),
    { initialValue: null },
  );

  // Daily data only fetched for the daily view, feeds the balance/cashflow charts
  private readonly dailyData = toSignal(
    this.period$.pipe(
      switchMap((period) => {
        if (period.viewMode !== 'daily') return [null];

        return combineLatest([
          this.summaryService.getDailyBalance({ accountId: period.accountId ?? undefined, month: period.month, year: period.year, currency: period.currency }),
          this.summaryService.getDailyCashflow({ accountId: period.accountId ?? undefined, month: period.month, year: period.year, currency: period.currency }),
        ]);
      }),
    ),
    { initialValue: null },
  );

  protected readonly selectedAccountId = computed(() => this.period().accountId);

  protected readonly balancePoints = computed<BalanceChartPoint[]>(() => {
    if (this.period().viewMode === 'daily') {
      const daily = this.dailyData();
      return (
        daily?.[0].dailyBalance.map((point) => ({
          label: point.date,
          total: point.balance,
          byAccount: point.byAccount,
        })) ?? []
      );
    }

    const monthly = this.monthlyData();
    return (
      monthly?.[0].points.map((point) => ({
        label: `${point.year}-${String(point.month).padStart(2, '0')}`,
        total: point.balance,
        byAccount: point.byAccount,
      })) ?? []
    );
  });

  protected readonly cashflowPoints = computed<CashflowChartPoint[]>(() => {
    if (this.period().viewMode === 'daily') {
      const daily = this.dailyData();
      return (
        daily?.[1].dailyClashFlow.map((point) => ({
          label: point.date,
          income: point.income,
          expenses: point.expenses,
          byAccount: point.byAccount,
        })) ?? []
      );
    }

    const monthly = this.monthlyData();
    return (
      monthly?.[1].points.map((point) => ({
        label: `${point.year}-${String(point.month).padStart(2, '0')}`,
        income: point.income,
        expenses: point.expenses,
        byAccount: point.byAccount,
      })) ?? []
    );
  });

  protected readonly netWorth = computed(() => {
    const points = this.monthlyData()?.[0].points ?? [];
    return points.at(-1)?.balance ?? null;
  });

  protected readonly currentIncome = computed(() => this.monthlyData()?.[1].points.at(-1)?.income ?? null);
  protected readonly previousIncome = computed(() => this.monthlyData()?.[1].points.at(-2)?.income ?? null);
  protected readonly currentExpenses = computed(() => this.monthlyData()?.[1].points.at(-1)?.expenses ?? null);
  protected readonly previousExpenses = computed(() => this.monthlyData()?.[1].points.at(-2)?.expenses ?? null);

  protected readonly rankingBalanceByAccount = computed(() => this.monthlyData()?.[0].points.at(-1)?.byAccount ?? null);
  protected readonly rankingCashflowByAccount = computed(() => this.monthlyData()?.[1].points.at(-1)?.byAccount ?? null);

  protected onPeriodChange(period: SummaryPeriod): void {
    this.period.set(period);
  }

  private getStoredCurrency(): string {
    return globalThis.localStorage?.getItem(Summary.CURRENCY_KEY)?.toUpperCase() || 'EUR';
  }
}
