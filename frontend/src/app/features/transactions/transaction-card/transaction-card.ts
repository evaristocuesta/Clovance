import { Component, computed, input, output } from '@angular/core';
import { Transaction } from '../models/transaction.model';
import { Icon } from "@shared/ui/icon/icon";
import { TranslocoDirective } from '@jsverse/transloco';

@Component({
  selector: 'app-transaction-card',
  imports: [TranslocoDirective, Icon],
  templateUrl: './transaction-card.html',
  styleUrl: './transaction-card.css',
})
export class TransactionCard {
  readonly transaction = input.required<Transaction>();
  readonly editTransaction = output<string>();
  readonly deleteTransaction = output<string>();

  protected isPositive = computed(() => this.transaction().amount >= 0);

  protected amountClass = computed(() => {
    const baseClass = 'flex h-14 min-w-[8rem] shrink-0 items-center justify-end rounded-2xl px-3 text-sm font-bold tabular-nums whitespace-nowrap transition-colors group-hover:text-white';

    return this.isPositive()
      ? `${baseClass} text-primary-700 bg-primary-100 group-hover:bg-primary-600`
      : `${baseClass} text-red-700 bg-red-200 group-hover:bg-red-600`;
  });

  protected signedAmount = computed(() => {
    const amount = this.transaction().amount;
    const formattedAmount = new Intl.NumberFormat(undefined, {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).format(Math.abs(amount));

    return amount >= 0 ? `+${formattedAmount}` : `-${formattedAmount}`;
  });

  protected typeLabel = computed(() => {
    const rawType = String(this.transaction().type).trim().toLowerCase();
    const mappedType = this.transactionTypeMap[rawType] ?? rawType;

    return mappedType;
  });

  protected formattedDate = computed(() => {
    const date = this.parseDate(this.transaction().date);

    if (!date) {
      return '';
    }

    return new Intl.DateTimeFormat(undefined, {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
    }).format(date);
  });

  private readonly transactionTypeMap: Record<string, 'income' | 'expense' | 'transfer'> = {
    '0': 'income',
    '1': 'expense',
    '2': 'transfer',
    income: 'income',
    expense: 'expense',
    transfer: 'transfer',
  };

  private parseDate(value: Date): Date | null {
    if (value instanceof Date) {
      return Number.isNaN(value.getTime()) ? null : value;
    }

    const parsedDate = new Date(value);

    return Number.isNaN(parsedDate.getTime()) ? null : parsedDate;
  }

  protected onEdit(): void {
    this.editTransaction.emit(this.transaction().id);
  }

  protected onDelete(): void {
    this.deleteTransaction.emit(this.transaction().id);
  }
}
