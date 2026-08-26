import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { TranslocoDirective } from '@jsverse/transloco';
import { Currency } from '@features/accounts/models/currency.model';
import { EChart, EChartsOption } from '@shared/ui/echart/echart';
import { formatCurrency } from '@shared/utils/currency-utils';
import { CashflowChartPoint } from '../models/summary-chart.model';

@Component({
  selector: 'app-cashflow-chart',
  imports: [EChart, TranslocoDirective],
  templateUrl: './cashflow-chart.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './cashflow-chart.css',
})
export class CashflowChart {
  readonly points = input<CashflowChartPoint[]>([]);
  readonly currency = input('EUR');
  readonly currencyOptions = input<Currency[]>([]);
  readonly language = input('en');

  protected readonly options = computed<EChartsOption>(() => {
    const points = this.points();

    return {
      tooltip: {
        trigger: 'axis',
        valueFormatter: (value) => formatCurrency(Number(value), this.currency(), this.currencyOptions(), this.language()),
      },
      legend: { top: 0 },
      grid: { left: 48, right: 24, top: 40, bottom: 24 },
      xAxis: { type: 'category', data: points.map((point) => point.label) },
      yAxis: {
        type: 'value',
        axisLabel: { formatter: (value) => formatCurrency(Number(value), this.currency(), this.currencyOptions(), this.language()) },
      },
      series: [
        {
          name: 'Income',
          type: 'bar',
          stack: 'cashflow',
          itemStyle: { color: '#677821' }, // primary-600
          data: points.map((point) => point.income),
        },
        {
          name: 'Expenses',
          type: 'bar',
          stack: 'cashflow',
          itemStyle: { color: '#fb2c36' }, // red-500
          data: points.map((point) => point.expenses),
        },
        {
          name: 'Net',
          type: 'line',
          itemStyle: { color: '#2563eb' },
          data: points.map((point) => point.income + point.expenses),
        },
      ],
    };
  });

}
