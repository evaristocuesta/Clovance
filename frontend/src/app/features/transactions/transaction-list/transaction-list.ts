import { afterNextRender, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { TranslocoModule, TranslocoService } from '@jsverse/transloco';
import { Icon } from "@shared/ui/icon/icon";
import { TransactionCard } from "../transaction-card/transaction-card";
import { Dialog, DialogRef } from '@angular/cdk/dialog';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { auditTime, distinctUntilChanged, filter, finalize, fromEvent, map, take, tap } from 'rxjs';
import { Transaction } from '../models/transaction.model';
import { TransactionFilters, TransactionService } from '../services/transaction.service';
import { AccountService } from '@features/accounts/services/account.service';
import { TransactionsFilter } from '../transactions-filter/transactions-filter';
import { TransactionForm, TransactionFormData } from '../transaction-form/transaction-form';
import { TransferForm } from '../transfer-form/transfer-form';
import { toTransactionType } from '../models/transaction-type.model';

@Component({
  selector: 'app-transaction-list',
  imports: [TranslocoModule, Icon, TransactionCard, TransactionsFilter],
  templateUrl: './transaction-list.html',
  styleUrl: './transaction-list.css',
})
export class TransactionList {
  private static readonly PAGE_SIZE = 20;
  private static readonly SCROLL_THRESHOLD_PX = 280;

  private readonly accountService = inject(AccountService);
  private readonly transactionService = inject(TransactionService);
  private readonly destroyRef = inject(DestroyRef);
  readonly translocoService = inject(TranslocoService);
  readonly dialog = inject(Dialog);
  private readonly cursorDate = signal<string | null>(null);
  private readonly cursorId = signal<string | null>(null);
  protected readonly hasMoreTransactions = signal(true);
  protected readonly isLoadingTransactions = signal(false);
  private readonly activeFilters = signal<TransactionFilters>({});

  readonly currencies = toSignal(this.accountService.getCurrencies(), { initialValue: [] });
  readonly accounts = toSignal(this.accountService.getAccounts(), { initialValue: [] });
  protected readonly transactions = signal<Transaction[] | null>(null);
  
  readonly currencySymbolMap = computed(() =>
    Object.fromEntries(this.currencies().map((c) => [c.code.toUpperCase(), c.symbol]))
  );

  constructor() {
    this.refreshTransactions();
    afterNextRender(() => this.setupScrollPagination());
  }

  private refreshTransactions(): void {
    this.cursorDate.set(null);
    this.cursorId.set(null);
    this.hasMoreTransactions.set(true);
    this.transactions.set(null);
    this.loadNextPage();
  }

  private setupScrollPagination(): void {
    fromEvent(window, 'scroll').pipe(
      auditTime(120),
      map(() => this.isNearBottom()),
      distinctUntilChanged(),
      filter((isNearBottom) => isNearBottom),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe(() => this.loadNextPage());
  }

  private isNearBottom(): boolean {
    const scrollPosition = window.scrollY + window.innerHeight;
    const fullHeight = document.documentElement.scrollHeight;

    return scrollPosition >= fullHeight - TransactionList.SCROLL_THRESHOLD_PX;
  }

  private loadNextPage(): void {
    if (!this.hasMoreTransactions() || this.isLoadingTransactions()) {
      return;
    }

    this.isLoadingTransactions.set(true);

    this.transactionService.getTransactionsPage({
      ...this.activeFilters(),
      cursorDate: this.cursorDate(),
      cursorId: this.cursorId(),
      pageSize: TransactionList.PAGE_SIZE,
    }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.isLoadingTransactions.set(false)),
    ).subscribe({
      next: (page) => {
        const sortedPage = [...page.items].sort((a, b) => {
          const dateComparison = b.date.getTime() - a.date.getTime();
          if (dateComparison !== 0) {
            return dateComparison;
          }

          return b.id.localeCompare(a.id);
        });

        this.transactions.update((current) => current ? [...current, ...sortedPage] : sortedPage);
        this.hasMoreTransactions.set(page.hasMore);
        this.cursorDate.set(page.nextCursorDate);
        this.cursorId.set(page.nextCursorId);
      },
      error: () => {
        if (this.transactions() === null) {
          this.transactions.set([]);
        }
      },
    });
  }

  onApplyFilters(filters: TransactionFilters): void {
    this.activeFilters.set(filters);
    this.refreshTransactions();
  }
  
  onAddTransfer(): void {
    const dialogRef = this.dialog.open<boolean>(TransferForm, {
          width: '640px',
          height: 'auto'
        });
    
        this.refreshOnDialogSuccess(dialogRef);
  }

  onAddExpense(): void {
    const dialogData: TransactionFormData = {
      type: 'expense',
      accounts: this.accounts(),
    };

    const dialogRef = this.dialog.open<boolean>(TransactionForm, {
          width: '640px',
          height: 'auto', 
          data: dialogData,
        });
    
        this.refreshOnDialogSuccess(dialogRef);
  }

  onAddIncome(): void {
    const dialogData: TransactionFormData = {
      type: 'income',
      accounts: this.accounts(),
    };

    const dialogRef = this.dialog.open<boolean>(TransactionForm, {
          width: '640px',
          height: 'auto', 
          data: dialogData,
        });
    
        this.refreshOnDialogSuccess(dialogRef);
  }

  private refreshOnDialogSuccess(dialogRef: DialogRef<boolean>): void {
    dialogRef.closed
      .pipe(
        take(1),  
        filter((result): result is true => result === true),
        tap(() => this.refreshTransactions()),
      )
      .subscribe();
  }

  onEdit(transaction: Transaction): void {
    const dialogData: TransactionFormData = {
      transaction,
      type: toTransactionType(transaction.type) || transaction.type,
      accounts: this.accounts(),
    };

    const dialogRef = this.dialog.open<boolean>(TransactionForm, {
          width: '640px',
          height: 'auto', 
          data: dialogData,
        });
    
        this.refreshOnDialogSuccess(dialogRef);
  }

  onDelete(transactionId: string): void {
  }
}
