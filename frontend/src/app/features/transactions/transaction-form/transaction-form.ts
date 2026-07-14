import { Component, inject, OnInit, signal } from '@angular/core';
import { Transaction } from '../models/transaction.model';
import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { SaveTransactionDto, TransactionService } from '../services/transaction.service';
import { firstValueFrom } from 'rxjs';
import { form, FormField, FormRoot, required, validate } from '@angular/forms/signals';
import { HttpErrorResponse } from '@angular/common/http';
import { Icon } from '@shared/ui/icon/icon';
import { TranslocoDirective } from '@jsverse/transloco';
import { Account } from '@features/accounts/models/account.model';

export interface TransactionFormData {
  transaction?: Transaction;
  type: 'Income' | 'Expense' | 'Transfer';
  accounts: Account[];
}

@Component({
  selector: 'app-transaction-form',
  imports: [TranslocoDirective, FormField, FormRoot, Icon],
  templateUrl: './transaction-form.html',
  styleUrl: './transaction-form.css',
})
export class TransactionForm implements OnInit {
  errorMessage = signal('');

  transaction = signal<Transaction>({
    id: '',
    date: new Date(),
    description: '',
    amount: 0,
    accountId: '',
    type: 'Income',
    accountName: '',
    currency: '',
  });

  private readonly transactionService = inject(TransactionService);

  dialogRef = inject<DialogRef>(DialogRef);
  data = inject<TransactionFormData>(DIALOG_DATA, { optional: true });

  ngOnInit(): void {
    const dataType = this.data?.type;

    if (dataType) {
      this.transaction.update((t) => ({ ...t, type: dataType }));
    }

    if (this.data?.transaction) {
      this.transaction.set(this.data.transaction);

      if (this.data?.type === 'Expense') {
        this.transaction.update((t) => ({ ...t, amount: Math.abs(t.amount) }));
      }

      if (dataType) {
        this.transaction.update((t) => ({ ...t, type: dataType }));
      }
    }
  }

  close() {
    this.dialogRef.close();
  }

  transactionForm = form(
    this.transaction,
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
      required(schemaPath.accountId, { message: 'transactions.accountRequired' });
      required(schemaPath.type, { message: 'transactions.typeRequired' });
    },
    { 
      submission: {
        action: async (field) => {
          this.errorMessage.set('');
          
          try {
            const formValue = field().value();

            const dto: SaveTransactionDto = {
              date: this.toDateOnlyString(formValue.date),
              description: formValue.description,
              amount: formValue.amount,
              type: formValue.type,
              accountId: formValue.accountId,
            };

            if (dto.type === 'Expense') {
              dto.amount = -Math.abs(dto.amount);
            }

            if (this.data?.transaction?.id) {
              await firstValueFrom(this.transactionService.updateTransaction(this.data.transaction.id, dto));
            } else {
              await firstValueFrom(this.transactionService.createTransaction(dto));
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