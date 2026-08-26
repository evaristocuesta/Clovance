import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { TranslocoDirective } from '@jsverse/transloco';
import { EChart, EChartsOption } from '@shared/ui/echart/echart';
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

  protected readonly options = computed<EChartsOption>(() => {
    const points = this.points();

    return {
      tooltip: { trigger: 'axis' },
      legend: { top: 0 },
      grid: { left: 48, right: 24, top: 40, bottom: 24 },
      xAxis: { type: 'category', data: points.map((point) => point.label) },
      yAxis: { type: 'value' },
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
