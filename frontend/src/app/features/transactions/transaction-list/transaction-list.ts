import { Component, inject, signal } from '@angular/core';
import { TranslocoModule, TranslocoService } from '@jsverse/transloco';
import { Icon } from "@shared/ui/icon/icon";
import { TransactionCard } from "../transaction-card/transaction-card";
import { Dialog } from '@angular/cdk/dialog';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { switchMap } from 'rxjs/internal/operators/switchMap';
import { map, startWith } from 'rxjs';
import { Transaction } from '../models/transaction.model';
import { TransactionService } from '../services/transaction.service';

@Component({
  selector: 'app-transaction-list',
  imports: [TranslocoModule, Icon, TransactionCard],
  templateUrl: './transaction-list.html',
  styleUrl: './transaction-list.css',
})
export class TransactionList {
  private readonly transactionService = inject(TransactionService);
  readonly translocoService = inject(TranslocoService);
  readonly dialog = inject(Dialog);
  private readonly refreshTick = signal(0);

  protected readonly transactions = toSignal(
    toObservable(this.refreshTick).pipe(
      switchMap(() => this.transactionService.getTransactions().pipe(
        map((transactions) => {
          const t = [...transactions].sort((a, b) => {
            const dateComparison = b.date.getTime() - a.date.getTime();
            if (dateComparison !== 0) {
              return dateComparison;
            }

            return b.id.localeCompare(a.id);
          });
          return t;
        }),
      )),
      startWith(null as Transaction[] | null),
    ),
  );

  private refreshTransactions(): void {
    this.refreshTick.update((value) => value + 1);
  }
  
  onAdd(): void {
  }

  onEdit(transactionId: string): void {
  }

  onDelete(transactionId: string): void {
  }
}
