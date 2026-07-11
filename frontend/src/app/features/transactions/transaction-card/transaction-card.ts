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
