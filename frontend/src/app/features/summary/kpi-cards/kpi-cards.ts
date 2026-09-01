import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { TranslocoDirective } from '@jsverse/transloco';
import { Currency } from '@features/accounts/models/currency.model';
import { formatCurrency } from '@shared/utils/currency-utils';

@Component({
  selector: 'app-kpi-cards',
  imports: [TranslocoDirective, DecimalPipe],
  templateUrl: './kpi-cards.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './kpi-cards.css',
})
export class KpiCards {
  readonly netWorth = input<number | null>(null);
  readonly previousNetWorth = input<number | null>(null);
  readonly income = input<number | null>(null);
  readonly previousIncome = input<number | null>(null);
  readonly expenses = input<number | null>(null);
  readonly previousExpenses = input<number | null>(null);
  readonly layout = input<'horizontal' | 'vertical'>('horizontal');
  readonly currency = input('EUR');
  readonly currencyOptions = input<Currency[]>([]);
  readonly language = input('en');

  protected readonly formattedNetWorth = computed(() => formatCurrency(this.netWorth() ?? 0, this.currency(), this.currencyOptions(), this.language()));
  protected readonly formattedIncome = computed(() => formatCurrency(this.income() ?? 0, this.currency(), this.currencyOptions(), this.language()));
  protected readonly formattedExpenses = computed(() => formatCurrency(this.expenses() ?? 0, this.currency(), this.currencyOptions(), this.language()));

  protected readonly netWorthVariation = computed(() => this.variation(this.netWorth(), this.previousNetWorth()));
  protected readonly incomeVariation = computed(() => this.variation(this.income(), this.previousIncome()));
  protected readonly expensesVariation = computed(() => this.variation(this.expenses(), this.previousExpenses()));

  private variation(current: number | null, previous: number | null): number | null {
    if (current == null || previous == null || previous === 0) return null;
    return ((current - previous) / Math.abs(previous)) * 100;
  }
}
