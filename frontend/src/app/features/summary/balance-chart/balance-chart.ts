import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { TranslocoDirective } from '@jsverse/transloco';
import { Account } from '@features/accounts/models/account.model';
import { EChart, EChartsOption } from '@shared/ui/echart/echart';
import { BalanceChartPoint } from '../models/summary-chart.model';

@Component({
  selector: 'app-balance-chart',
  imports: [EChart, TranslocoDirective],
  templateUrl: './balance-chart.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './balance-chart.css',
})
export class BalanceChart {
  readonly points = input<BalanceChartPoint[]>([]);
  readonly accounts = input<Account[]>([]);
  readonly selectedAccountId = input<string | null>(null);

  private readonly accountNameById = computed(() =>
    Object.fromEntries(this.accounts().map((account) => [account.id, account.name])),
  );

  protected readonly options = computed<EChartsOption>(() => {
    const points = this.points();
    const labels = points.map((point) => point.label);
    const accountId = this.selectedAccountId();

    const series = accountId
      ? [
          {
            name: this.accountNameById()[accountId] ?? accountId,
            type: 'line' as const,
            areaStyle: {},
            data: points.map((point) => point.total),
          },
        ]
      : this.buildAccountSeries(points);

    return {
      tooltip: { trigger: 'axis' },
      legend: { top: 0 },
      grid: { left: 48, right: 24, top: 40, bottom: 72 },
      xAxis: { type: 'category', data: labels, boundaryGap: false },
      yAxis: { type: 'value' },
      dataZoom: [
        { type: 'slider', start: 0, end: 100, bottom: 16, height: 24 },
        { type: 'inside' },
      ],
      series,
    };
  });

  private buildAccountSeries(points: BalanceChartPoint[]) {
    const accountIds = new Set<string>();
    for (const point of points) {
      for (const entry of point.byAccount ?? []) {
        accountIds.add(entry.accountId);
      }
    }

    const nameById = this.accountNameById();

    return Array.from(accountIds).map((accountId) => ({
      name: nameById[accountId] ?? accountId,
      type: 'line' as const,
      stack: 'balance',
      areaStyle: {},
      emphasis: { focus: 'series' as const },
      data: points.map((point) => point.byAccount?.find((entry) => entry.accountId === accountId)?.balance ?? 0),
    }));
  }
}
