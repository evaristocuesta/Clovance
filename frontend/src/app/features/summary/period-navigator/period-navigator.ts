import { ChangeDetectionStrategy, Component, computed, effect, input, output, signal } from '@angular/core';
import { TranslocoDirective } from '@jsverse/transloco';
import { Account } from '@features/accounts/models/account.model';
import { Icon } from '@shared/ui/icon/icon';
import { SummaryPeriod } from '../models/summary-period.model';

@Component({
  selector: 'app-period-navigator',
  imports: [TranslocoDirective, Icon],
  templateUrl: './period-navigator.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './period-navigator.css',
})
export class PeriodNavigator {
  readonly accounts = input<Account[]>([]);
  readonly periodChange = output<SummaryPeriod>();

  private readonly today = new Date();
  protected readonly year = signal(this.today.getFullYear());
  protected readonly month = signal(this.today.getMonth() + 1);
  protected readonly viewMode = signal<'monthly' | 'daily'>('monthly');
  protected readonly accountId = signal<string | null>(null);

  protected readonly activeAccounts = computed(() => this.accounts().filter((account) => !account.isDeleted));

  protected readonly periodLabel = computed(() => {
    const date = new Date(this.year(), this.month() - 1, 1);
    return date.toLocaleDateString(undefined, { month: 'long', year: 'numeric' });
  });

  constructor() {
    effect(() => {
      this.periodChange.emit({
        year: this.year(),
        month: this.month(),
        viewMode: this.viewMode(),
        accountId: this.accountId(),
      });
    });
  }

  protected previousMonth(): void {
    this.shiftMonth(-1);
  }

  protected nextMonth(): void {
    this.shiftMonth(1);
  }

  protected setViewMode(mode: 'monthly' | 'daily'): void {
    this.viewMode.set(mode);
  }

  protected onAccountChange(value: string): void {
    this.accountId.set(value || null);
  }

  private shiftMonth(delta: number): void {
    const date = new Date(this.year(), this.month() - 1 + delta, 1);
    this.year.set(date.getFullYear());
    this.month.set(date.getMonth() + 1);
  }
}
