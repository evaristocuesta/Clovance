import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { combineLatest, distinctUntilChanged, map, switchMap } from 'rxjs';
import { TranslocoDirective, TranslocoService } from '@jsverse/transloco';
import { ActivatedRoute, Router } from '@angular/router';
import { AccountService } from '@features/accounts/services/account.service';
import { Currency } from '@features/accounts/models/currency.model';
import { SummaryService } from '../services/summary.service';
import { PeriodNavigator } from '../period-navigator/period-navigator';
import { KpiCards } from '../kpi-cards/kpi-cards';
import { LiabilityKpis } from '../liability-kpis/liability-kpis';
import { BalanceChart } from '../balance-chart/balance-chart';
import { CashflowChart } from '../cashflow-chart/cashflow-chart';
import { AccountRanking } from '../account-ranking/account-ranking';
import { SummaryPeriod } from '../models/summary-period.model';
import { BalanceChartPoint, CashflowChartPoint } from '../models/summary-chart.model';
import { SummaryTab, getSummaryAccounts, toBalanceViewPoints, toCashflowViewPoints } from '../utils/summary-view.utils';

@Component({
  selector: 'app-summary',
  imports: [TranslocoDirective, PeriodNavigator, KpiCards, LiabilityKpis, BalanceChart, CashflowChart, AccountRanking],
  templateUrl: './summary.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './summary.css',
})
export class Summary {
  private static readonly CURRENCY_KEY = 'summary-currency';

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly accountService = inject(AccountService);
  private readonly summaryService = inject(SummaryService);
  private readonly translocoService = inject(TranslocoService);

  protected readonly accounts = toSignal(this.accountService.getAccounts(), { initialValue: [] });
  protected readonly currencies = toSignal(this.accountService.getCurrencies(), { initialValue: [] as Currency[] });
  protected readonly language = toSignal(this.translocoService.langChanges$, {
    initialValue: this.translocoService.getActiveLang(),
  });
  protected readonly activeTab = toSignal(
    this.route.queryParamMap.pipe(
      map((params): SummaryTab => params.get('tab') === 'liabilities' ? 'liabilities' : 'assets'),
    ),
    { initialValue: 'assets' },
  );
  protected readonly summaryAccounts = computed(() => getSummaryAccounts(this.accounts(), this.activeTab()));
  private readonly accountType = computed(() => this.activeTab() === 'liabilities' ? 'liability' : 'asset');

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
  private readonly summaryRequest$ = combineLatest([this.period$, toObservable(this.accountType)]);

  // Monthly data always fetched: feeds KPIs, ranking, and the charts in monthly view
  private readonly monthlyData = toSignal(
    this.summaryRequest$.pipe(
      switchMap(([period, accountType]) =>
        combineLatest([
          this.summaryService.getMonthlyBalance({ accountId: period.accountId ?? undefined, accountType, month: period.month, year: period.year, currency: period.currency }),
          this.summaryService.getMonthlyCashflow({ accountId: period.accountId ?? undefined, accountType, month: period.month, year: period.year, currency: period.currency }),
        ]),
      ),
    ),
    { initialValue: null },
  );

  // Daily data only fetched for the daily view, feeds the balance/cashflow charts
  private readonly dailyData = toSignal(
    this.summaryRequest$.pipe(
      switchMap(([period, accountType]) => {
        if (period.viewMode !== 'daily') return [null];

        return combineLatest([
          this.summaryService.getDailyBalance({ accountId: period.accountId ?? undefined, accountType, month: period.month, year: period.year, currency: period.currency }),
          this.summaryService.getDailyCashflow({ accountId: period.accountId ?? undefined, accountType, month: period.month, year: period.year, currency: period.currency }),
        ]);
      }),
    ),
    { initialValue: null },
  );

  protected readonly selectedAccountId = computed(() => this.period().accountId);

  protected readonly balancePoints = computed<BalanceChartPoint[]>(() => {
    if (this.period().viewMode === 'daily') {
      const daily = this.dailyData();
      const points =
        daily?.[0].dailyBalance.map((point) => ({
          label: point.date,
          total: point.balance,
          byAccount: point.byAccount,
        })) ?? [];
      return toBalanceViewPoints(points, this.activeTab());
    }

    const monthly = this.monthlyData();
    const points =
      monthly?.[0].points.map((point) => ({
        label: `${point.year}-${String(point.month).padStart(2, '0')}`,
        total: point.balance,
        byAccount: point.byAccount,
      })) ?? [];
    return toBalanceViewPoints(points, this.activeTab());
  });

  protected readonly cashflowPoints = computed<CashflowChartPoint[]>(() => {
    if (this.period().viewMode === 'daily') {
      const daily = this.dailyData();
      const points =
        daily?.[1].dailyClashFlow.map((point) => ({
          label: point.date,
          income: point.income,
          expenses: point.expenses,
          byAccount: point.byAccount,
        })) ?? [];
      return toCashflowViewPoints(points, this.activeTab());
    }

    const monthly = this.monthlyData();
    const points =
      monthly?.[1].points.map((point) => ({
        label: `${point.year}-${String(point.month).padStart(2, '0')}`,
        income: point.income,
        expenses: point.expenses,
        byAccount: point.byAccount,
      })) ?? [];
    return toCashflowViewPoints(points, this.activeTab());
  });

  protected readonly netWorth = computed(() => {
    const points = this.monthlyData()?.[0].points ?? [];
    const balance = points.at(-1)?.balance;
    return balance == null || this.activeTab() === 'assets' ? balance ?? null : Math.abs(balance);
  });
  protected readonly previousNetWorth = computed(() => this.monthlyData()?.[0].points.at(-2)?.balance ?? null);
  protected readonly debt = computed(() => this.toLiabilityAmount(this.monthlyData()?.[0].points.at(-1)?.balance));
  protected readonly previousDebt = computed(() => this.toLiabilityAmount(this.monthlyData()?.[0].points.at(-2)?.balance));
  protected readonly principalPaid = computed(() => this.toLiabilityAmount(this.monthlyData()?.[1].points.at(-1)?.income));
  protected readonly previousPrincipalPaid = computed(() => this.toLiabilityAmount(this.monthlyData()?.[1].points.at(-2)?.income));
  protected readonly newDebt = computed(() => this.toLiabilityAmount(this.monthlyData()?.[1].points.at(-1)?.expenses));
  protected readonly previousNewDebt = computed(() => this.toLiabilityAmount(this.monthlyData()?.[1].points.at(-2)?.expenses));

  protected readonly currentIncome = computed(() => this.toViewAmount(this.monthlyData()?.[1].points.at(-1)?.income));
  protected readonly previousIncome = computed(() => this.toViewAmount(this.monthlyData()?.[1].points.at(-2)?.income));
  protected readonly currentExpenses = computed(() => this.toViewAmount(this.monthlyData()?.[1].points.at(-1)?.expenses));
  protected readonly previousExpenses = computed(() => this.toViewAmount(this.monthlyData()?.[1].points.at(-2)?.expenses));

  protected readonly rankingBalanceByAccount = computed(() => {
    const byAccount = this.monthlyData()?.[0].points.at(-1)?.byAccount;
    return toBalanceViewPoints([{ label: '', total: 0, byAccount: byAccount ?? null }], this.activeTab())[0].byAccount;
  });

  protected readonly rankingCashflowByAccount = computed(() => {
    const byAccount = this.monthlyData()?.[1].points.at(-1)?.byAccount;
    return toCashflowViewPoints([{ label: '', income: 0, expenses: 0, byAccount: byAccount ?? null }], this.activeTab())[0].byAccount;
  });

  protected onPeriodChange(period: SummaryPeriod): void {
    this.period.set(period);
  }

  protected selectTab(tab: 'assets' | 'liabilities'): void {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab },
      queryParamsHandling: 'merge',
    });
  }

  private toViewAmount(amount: number | undefined): number | null {
    if (amount == null) return null;
    return this.activeTab() === 'liabilities' ? Math.abs(amount) : amount;
  }

  private toLiabilityAmount(amount: number | undefined): number | null {
    return amount == null ? null : Math.abs(amount);
  }

  private getStoredCurrency(): string {
    return globalThis.localStorage?.getItem(Summary.CURRENCY_KEY)?.toUpperCase() || 'EUR';
  }
}
