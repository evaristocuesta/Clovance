import { Component, computed, ElementRef, inject, input, output, signal } from '@angular/core';
import { Transaction } from '../models/transaction.model';
import { Icon } from "@shared/ui/icon/icon";
import { TranslocoDirective } from '@jsverse/transloco';

@Component({
  selector: 'app-transaction-card',
  imports: [TranslocoDirective, Icon],
  templateUrl: './transaction-card.html',
  styleUrl: './transaction-card.css',
  host: {
    '(document:click)': 'onDocumentClick($event)',
  },
})
export class TransactionCard {
  private readonly elementRef = inject(ElementRef<HTMLElement>);

  readonly transaction = input.required<Transaction>();
  readonly currencySymbolMap = input.required<Record<string, string>>();
  readonly editTransaction = output<Transaction>();
  readonly deleteTransaction = output<string>();
  protected isActionsMenuOpen = signal(false);

  protected isPositive = computed(() => this.transaction().amount >= 0);

  protected amountClass = computed(() => {
    const baseClass = 'flex h-9 min-w-[9rem] shrink-0 items-center justify-end rounded-2xl px-3 text-sm font-bold tabular-nums whitespace-nowrap transition-colors group-hover:text-white';

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

  private parseDate(value: Date): Date | null {
    if (value instanceof Date) {
      return Number.isNaN(value.getTime()) ? null : value;
    }

    const parsedDate = new Date(value);

    return Number.isNaN(parsedDate.getTime()) ? null : parsedDate;
  }

  protected currencyLabel = computed(() => 
    this.currencySymbolMap()[this.transaction().currency.toUpperCase()] ?
    `${this.transaction().currency.toUpperCase()} ${this.currencySymbolMap()[this.transaction().currency.toUpperCase()]}` 
    : this.transaction().currency.toUpperCase());

  protected onEdit(): void {
    this.isActionsMenuOpen.set(false);
    this.editTransaction.emit(this.transaction());
  }

  protected onDelete(): void {
    this.isActionsMenuOpen.set(false);
    this.deleteTransaction.emit(this.transaction().id);
  }

  protected toggleActionsMenu(): void {
    this.isActionsMenuOpen.update((isOpen) => !isOpen);
  }

  protected onDocumentClick(event: Event): void {
    const target = event.target;

    if (!(target instanceof Node)) {
      return;
    }

    if (!this.elementRef.nativeElement.contains(target)) {
      this.isActionsMenuOpen.set(false);
    }
  }
}
