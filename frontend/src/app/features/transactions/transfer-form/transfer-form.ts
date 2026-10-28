import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { Transfer } from '../models/transfer.model';
import { Account } from '@features/accounts/models/account.model';
import { Transaction } from '../models/transaction.model';
import {
  SaveTransferCommand,
  TransactionService,
  UpdateTransferCommand,
} from '../services/transaction.service';
import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { form, FormField, FormRoot, required, validate } from '@angular/forms/signals';
import { HttpErrorResponse } from '@angular/common/http';
import { Icon } from '@shared/ui/icon/icon';
import { TranslocoDirective, TranslocoService } from '@jsverse/transloco';
import { ToastService } from '@shared/ui/toast/toast.service';
import { firstValueFrom, forkJoin } from 'rxjs';
import { toDateOnlyString } from '@shared/utils/date-utils';
import { Dialog } from '@shared/ui/dialog/dialog';

export interface TransferFormData {
  toTransaction?: Transaction;
  fromTransaction?: Transaction;
  accounts: Account[];
}

@Component({
  selector: 'app-transfer-form',
  imports: [TranslocoDirective, FormField, FormRoot, Icon, Dialog],
  templateUrl: './transfer-form.html',
  styleUrl: './transfer-form.css',
})
export class TransferForm implements OnInit {
  errorMessage = signal('');

  fromAccounts = computed(
    () =>
      this.data?.accounts?.filter(
        (a) =>
          (Account.canBeUsedForTransfer(a) && !a.isDeleted) ||
          (a.id === this.data?.fromTransaction?.accountId && a.isDeleted),
      ) ?? [],
  );

  toAccounts = computed(
    () =>
      this.data?.accounts?.filter(
        (a) =>
          (Account.canBeUsedForTransfer(a) && !a.isDeleted) ||
          (a.id === this.data?.toTransaction?.accountId && a.isDeleted),
      ) ?? [],
  );

  transfer = signal<Transfer>({
    date: new Date(),
    description: '',
    amount: 0,
    fromAccountId: '',
    toAccountId: '',
  });

  /** The pair being edited. Holds the versions the save is checked against. */
  transferLegs = signal<{ toTransaction: Transaction; fromTransaction: Transaction } | null>(null);

  private readonly transactionService = inject(TransactionService);
  private readonly toastService = inject(ToastService);
  private readonly translocoService = inject(TranslocoService);

  dialogRef = inject<DialogRef>(DialogRef);
  data = inject<TransferFormData>(DIALOG_DATA, { optional: true });

  ngOnInit(): void {
    if (this.data?.fromTransaction && this.data?.toTransaction) {
      const legs = {
        toTransaction: this.data.toTransaction,
        fromTransaction: this.data.fromTransaction,
      };

      this.transferLegs.set(legs);
      this.applyTransferLegs(legs);

      // Re-read both legs so the versions sent on save are the ones the user is looking
      // at, not whatever the in-memory list last held. Without this a stale list makes
      // the save fail permanently: reopening the dialog would reuse the same outdated
      // versions and hit the same 409 again.
      forkJoin({
        toTransaction: this.transactionService.getTransactionById(legs.toTransaction.id),
        fromTransaction: this.transactionService.getTransactionById(legs.fromTransaction.id),
      }).subscribe({
        next: (freshLegs) => {
          this.transferLegs.set(freshLegs);
          this.applyTransferLegs(freshLegs);
        },
        error: () => {
          // Fall back to the legs the dialog was opened with; the save is still refused
          // if they turn out to be stale.
        },
      });
    }
  }

  private applyTransferLegs(legs: {
    toTransaction: Transaction;
    fromTransaction: Transaction;
  }): void {
    this.transfer.set({
      date: legs.fromTransaction.date,
      description: legs.fromTransaction.description,
      amount: legs.toTransaction.amount,
      fromAccountId: legs.fromTransaction.accountId,
      toAccountId: legs.toTransaction.accountId,
    });
  }

  close() {
    this.dialogRef.close();
  }

  transferForm = form(
    this.transfer,
    (schemaPath) => {
      required(schemaPath.date, { message: 'transactions.dateRequired' });
      required(schemaPath.description, { message: 'transactions.descriptionRequired' });
      required(schemaPath.amount, { message: 'transactions.amountRequired' });
      validate(schemaPath.amount, ({ value }) => {
        const amount = value();

        if (amount <= 0) {
          return {
            kind: 'amountGreaterThanZero',
            message: 'transactions.amountGreaterThanZero',
          };
        }

        return null;
      });
      required(schemaPath.fromAccountId, { message: 'transactions.fromAccountRequired' });
      required(schemaPath.toAccountId, { message: 'transactions.toAccountRequired' });
      validate(schemaPath.toAccountId, ({ value, valueOf }) => {
        const toAccountId = value();
        const fromAccountId = valueOf(schemaPath.fromAccountId);

        if (toAccountId && fromAccountId && toAccountId === fromAccountId) {
          return {
            kind: 'toAccountNotSameAsFromAccount',
            message: 'transactions.toAccountNotSameAsFromAccount',
          };
        }

        return null;
      });
    },
    {
      submission: {
        action: async (field) => {
          this.errorMessage.set('');

          try {
            const formValue = field().value();
            const legs = this.transferLegs();

            if (legs) {
              const dto: UpdateTransferCommand = {
                date: toDateOnlyString(formValue.date),
                description: formValue.description,
                amount: formValue.amount,
                fromAccountId: formValue.fromAccountId,
                toAccountId: formValue.toAccountId,
                // The URL addresses one leg of the pair, so its version goes in "version"
                // and the other leg's in "relatedVersion". Sending back the versions the
                // form was filled from lets the API refuse a save made from stale data.
                version: legs.toTransaction.version,
                relatedVersion: legs.fromTransaction.version,
              };

              await firstValueFrom(
                this.transactionService.updateTransfer(legs.toTransaction.id, dto),
              );
            } else {
              const dto: SaveTransferCommand = {
                date: toDateOnlyString(formValue.date),
                description: formValue.description,
                amount: formValue.amount,
                fromAccountId: formValue.fromAccountId,
                toAccountId: formValue.toAccountId,
              };

              await firstValueFrom(this.transactionService.createTransfer(dto));
            }

            this.toastService.success(this.translocoService.translate('transactions.saved'));
            this.dialogRef.close(true);
          } catch (err: HttpErrorResponse | any) {
            const errorCode = (err as { error: { errorCode?: string } })?.error?.errorCode;
            const key = errorCode ? errorCode : 'transactions.serverError';
            this.errorMessage.set(key);
          }
        },
      },
    },
  );
}
