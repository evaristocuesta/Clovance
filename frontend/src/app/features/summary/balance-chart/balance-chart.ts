import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { TranslocoDirective, TranslocoService } from '@jsverse/transloco';
import { Account } from '@features/accounts/models/account.model';
import { Currency } from '@features/accounts/models/currency.model';
import { EChart, EChartsOption } from '@shared/ui/echart/echart';
import { formatCurrency } from '@shared/utils/currency-utils';
import { BalanceChartPoint } from '../models/summary-chart.model';

@Component({
  selector: 'app-balance-chart',
  imports: [EChart, TranslocoDirective],
  templateUrl: './balance-chart.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './balance-chart.css',
})
export class BalanceChart {
  private readonly translocoService = inject(TranslocoService);

  readonly points = input<BalanceChartPoint[]>([]);
  readonly accounts = input<Account[]>([]);
  readonly selectedAccountId = input<string | null>(null);
  readonly currency = input('EUR');
  readonly currencyOptions = input<Currency[]>([]);
  readonly language = input('en');
  readonly totalLabelKey = input('summary.charts.totalBalance');

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
      tooltip: {
        trigger: 'axis',
        order: 'seriesDesc',
        formatter: accountId
          ? undefined
          : (params) => {
              const entries = Array.isArray(params) ? params : [params];
              const point = points[entries[0]?.dataIndex ?? 0];
              const totalLabel = this.translocoService.translate(this.totalLabelKey(), {}, this.language());
              const totalLine = `<strong>${entries[0]?.marker ?? ''}${totalLabel}: ${formatCurrency(point?.total ?? 0, this.currency(), this.currencyOptions(), this.language())}</strong><br/>`;
              const accountLines = [];

              for (const entry of [...entries].reverse()) {
                accountLines.push(`${entry.marker ?? ''}${entry.seriesName}: ${formatCurrency(Number(entry.value), this.currency(), this.currencyOptions(), this.language())}`);
              }

              return [point?.label ?? '', `${totalLine}${accountLines.join('<br/>')}`].join('<br/>');
            },
        valueFormatter: (value) => formatCurrency(Number(value), this.currency(), this.currencyOptions(), this.language()),
      },
      legend: { top: 0, data: series.map((entry) => entry.name).reverse() },
      grid: { left: 48, right: 24, top: 40, bottom: 72 },
      xAxis: { type: 'category', data: labels, boundaryGap: false },
      yAxis: {
        type: 'value',
        axisLabel: { formatter: (value) => formatCurrency(Number(value), this.currency(), this.currencyOptions(), this.language()) },
      },
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

    return Array.from(accountIds)
      .map((accountId) => ({
        accountId,
        data: points.map((point) => point.byAccount?.find((entry) => entry.accountId === accountId)?.balance ?? 0),
      }))
      .filter(({ data }) => data.some((balance) => balance !== 0))
      .map(({ accountId, data }) => ({
        name: nameById[accountId] ?? accountId,
        type: 'line' as const,
        stack: 'balance',
        areaStyle: {},
        emphasis: { focus: 'series' as const },
        data,
      }))
      .reverse();
  }
}
