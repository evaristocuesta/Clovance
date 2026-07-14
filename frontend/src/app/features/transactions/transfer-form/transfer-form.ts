import { Component, inject, OnInit, signal } from '@angular/core';
import { Transfer } from '../models/transfer.model';
import { Account } from '@features/accounts/models/account.model';
import { Transaction } from '../models/transaction.model';
import { SaveTransactionDto, SaveTransferCommand, TransactionService } from '../services/transaction.service';
import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { form, FormField, FormRoot, required, validate } from '@angular/forms/signals';
import { HttpErrorResponse } from '@angular/common/http';
import { Icon } from '@shared/ui/icon/icon';
import { TranslocoDirective } from '@jsverse/transloco';
import { firstValueFrom } from 'rxjs';

export interface TransferFormData {
  toTransaction?: Transaction;
  fromTransaction?: Transaction;
  accounts: Account[];
}

@Component({
  selector: 'app-transfer-form',
  imports: [TranslocoDirective, FormField, FormRoot, Icon],
  templateUrl: './transfer-form.html',
  styleUrl: './transfer-form.css',
})
export class TransferForm implements OnInit {
  errorMessage = signal('');

  transfer = signal<Transfer>({
    date: new Date(),
    description: '',
    amount: 0,
    fromAccountId: '',
    toAccountId: '',
  });

  private readonly transactionService = inject(TransactionService);

  dialogRef = inject<DialogRef>(DialogRef);
  data = inject<TransferFormData>(DIALOG_DATA, { optional: true });

  ngOnInit(): void {
    if (this.data?.fromTransaction && this.data?.toTransaction) {
      this.transfer.set({
        date: this.data.fromTransaction.date,
        description: this.data.fromTransaction.description,
        amount: this.data.toTransaction.amount,
        fromAccountId: this.data.fromTransaction.accountId,
        toAccountId: this.data.toTransaction.accountId
      });
    }
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

            

            if (this.data?.toTransaction?.id) {
              const dto: SaveTransferCommand = {
                date: this.toDateOnlyString(formValue.date),
                description: formValue.description,
                amount: formValue.amount,
                fromAccountId: formValue.fromAccountId,
                toAccountId: formValue.toAccountId,
              };

              await firstValueFrom(this.transactionService.updateTransfer(this.data.toTransaction.id, dto));
            } else {
              const dto: SaveTransferCommand = {
                date: this.toDateOnlyString(formValue.date),
                description: formValue.description,
                amount: formValue.amount,
                fromAccountId: formValue.fromAccountId,
                toAccountId: formValue.toAccountId,
              };

              await firstValueFrom(this.transactionService.createTransfer(dto));
            }
            this.dialogRef.close(true);
          } catch (err: HttpErrorResponse | any) {
              const errorCode = (err as { error: { errorCode?: string } })?.error?.errorCode;
              const key = errorCode ? errorCode : 'transactions.serverError';
              this.errorMessage.set(key);
            }
      }
    },
    }
  );

  private toDateOnlyString(value: Date | string): string {
    if (typeof value === 'string') {
      return value.length >= 10 ? value.slice(0, 10) : value;
    }

    const year = value.getFullYear();
    const month = String(value.getMonth() + 1).padStart(2, '0');
    const day = String(value.getDate()).padStart(2, '0');

    return `${year}-${month}-${day}`;
  }
}
