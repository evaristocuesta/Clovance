import { ChangeDetectionStrategy, Component, DestroyRef, ElementRef, afterNextRender, effect, inject, input, signal, viewChild } from '@angular/core';
import * as echarts from 'echarts/core';
import { BarChart, LineChart } from 'echarts/charts';
import { DataZoomComponent, GridComponent, LegendComponent, TooltipComponent } from 'echarts/components';
import { CanvasRenderer } from 'echarts/renderers';
import type { ComposeOption } from 'echarts/core';
import type { BarSeriesOption, LineSeriesOption } from 'echarts/charts';
import type { DataZoomComponentOption, GridComponentOption, LegendComponentOption, TooltipComponentOption } from 'echarts/components';
import { ThemeService } from '@core/services/theme.service';

echarts.use([BarChart, LineChart, DataZoomComponent, GridComponent, LegendComponent, TooltipComponent, CanvasRenderer]);

export type EChartsOption = ComposeOption<
  BarSeriesOption | LineSeriesOption | DataZoomComponentOption | GridComponentOption | LegendComponentOption | TooltipComponentOption
>;

@Component({
  selector: 'app-echart',
  template: `<div #container class="h-full w-full"></div>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EChart {
  readonly options = input.required<EChartsOption>();

  private readonly container = viewChild.required<ElementRef<HTMLDivElement>>('container');
  private readonly themeService = inject(ThemeService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly ready = signal(false);
  private chart: echarts.ECharts | null = null;
  private chartTheme: 'light' | 'dark' | null = null;

  constructor() {
    afterNextRender(() => {
      const element = this.container().nativeElement;
      const resizeObserver = new ResizeObserver(() => this.chart?.resize());
      resizeObserver.observe(element);

      this.destroyRef.onDestroy(() => {
        resizeObserver.disconnect();
        this.chart?.dispose();
      });

      this.ready.set(true);
    });

    // echarts themes can't be swapped on a live instance, so re-init whenever options or theme change
    effect(() => {
      const options = this.options();
      const isDark = this.themeService.theme() === 'dark';

      if (!this.ready()) return;

      const theme = isDark ? 'dark' : 'light';
      if (!this.chart || this.chartTheme !== theme) {
        this.chart?.dispose();
        this.chart = echarts.init(this.container().nativeElement, isDark ? 'dark' : undefined);
        this.chartTheme = theme;
      }

      this.chart.setOption(options, { notMerge: true });
    });
  }
}
