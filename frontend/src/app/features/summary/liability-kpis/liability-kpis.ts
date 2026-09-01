import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { TranslocoDirective } from '@jsverse/transloco';
import { Currency } from '@features/accounts/models/currency.model';
import { formatCurrency } from '@shared/utils/currency-utils';

@Component({
  selector: 'app-liability-kpis',
  imports: [TranslocoDirective, DecimalPipe],
  templateUrl: './liability-kpis.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './liability-kpis.css',
})
export class LiabilityKpis {
  readonly debt = input<number | null>(null);
  readonly previousDebt = input<number | null>(null);
  readonly principalPaid = input<number | null>(null);
  readonly previousPrincipalPaid = input<number | null>(null);
  readonly newDebt = input<number | null>(null);
  readonly previousNewDebt = input<number | null>(null);
  readonly currency = input('EUR');
  readonly currencyOptions = input<Currency[]>([]);
  readonly language = input('en');

  protected readonly formattedDebt = computed(() => formatCurrency(this.debt() ?? 0, this.currency(), this.currencyOptions(), this.language()));
  protected readonly formattedPrincipalPaid = computed(() => formatCurrency(this.principalPaid() ?? 0, this.currency(), this.currencyOptions(), this.language()));
  protected readonly formattedNewDebt = computed(() => formatCurrency(this.newDebt() ?? 0, this.currency(), this.currencyOptions(), this.language()));
  protected readonly debtVariation = computed(() => this.variation(this.debt(), this.previousDebt()));
  protected readonly principalPaidVariation = computed(() => this.variation(this.principalPaid(), this.previousPrincipalPaid()));
  protected readonly newDebtVariation = computed(() => this.variation(this.newDebt(), this.previousNewDebt()));

  private variation(current: number | null, previous: number | null): number | null {
    if (current == null || previous == null || previous === 0) return null;
    return ((current - previous) / Math.abs(previous)) * 100;
  }
}
