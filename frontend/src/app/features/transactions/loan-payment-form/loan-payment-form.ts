import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { DialogRef, DIALOG_DATA } from '@angular/cdk/dialog';
import { Account } from '@features/accounts/models/account.model';
import { Transaction } from '../models/transaction.model';
import { LoanPayment } from '../models/loan-payment.model';
import { SaveLoanPaymentCommand, TransactionService } from '../services/transaction.service';
import { form, FormField, FormRoot, required, validate } from '@angular/forms/signals';
import { toDateOnlyString } from '@shared/utils/date-utils';
import { HttpErrorResponse } from '@angular/common/http';
import { Icon } from '@shared/ui/icon/icon';
import { TranslocoDirective } from '@jsverse/transloco';
import { firstValueFrom } from 'rxjs';

export interface LoanPaymentFormData {
  toTransaction?: Transaction;
  fromTransaction?: Transaction;
  accounts: Account[];
}

@Component({
  selector: 'app-loan-payment-form',
  imports: [TranslocoDirective, FormField, FormRoot, Icon],
  templateUrl: './loan-payment-form.html',
  styleUrl: './loan-payment-form.css',
})
export class LoanPaymentForm implements OnInit {
  errorMessage = signal('');

  fromAccounts = computed(() => this.data?.accounts?.filter(Account.canBeUsedForFromLoanPayment) ?? []);
  toAccounts = computed(() => this.data?.accounts?.filter(Account.canBeUsedForToLoanPayment) ?? []);

  loanPayment = signal<LoanPayment>({
    date: new Date(),
    description: '',
    amount: 0,
    principalAmount: 0,
    fromAccountId: '',
    toAccountId: '',
  });

  private readonly transactionService = inject(TransactionService);

  dialogRef = inject<DialogRef>(DialogRef);
  data = inject<LoanPaymentFormData>(DIALOG_DATA, { optional: true });

  ngOnInit(): void {
    if (this.data?.fromTransaction && this.data?.toTransaction) {
      this.loanPayment.set({
        date: this.data.fromTransaction.date,
        description: this.data.fromTransaction.description,
        amount: -this.data.fromTransaction.amount,
        principalAmount: -(this.data.fromTransaction.principalAmount || 0),
        fromAccountId: this.data.fromTransaction.accountId,
        toAccountId: this.data.toTransaction.accountId
      });
    }
  }

  close() {
    this.dialogRef.close();
  }

  loanPaymentForm = form(
    this.loanPayment,
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
      required(schemaPath.principalAmount, { message: 'transactions.amountRequired' });
      validate(schemaPath.principalAmount, ({ value }) => {
        const principalAmount = value();

        if (principalAmount <= 0) {
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
              const command: SaveLoanPaymentCommand = {
                date: toDateOnlyString(formValue.date),
                description: formValue.description,
                amount: formValue.amount,
                principalAmount: formValue.principalAmount,
                fromAccountId: formValue.fromAccountId,
                toAccountId: formValue.toAccountId,
              };

              await firstValueFrom(this.transactionService.updateLoanPayment(this.data.toTransaction.id, command));
            } else {
              const command: SaveLoanPaymentCommand = {
                date: toDateOnlyString(formValue.date),
                description: formValue.description,
                amount: formValue.amount,
                principalAmount: formValue.principalAmount,
                fromAccountId: formValue.fromAccountId,
                toAccountId: formValue.toAccountId,
              };

              await firstValueFrom(this.transactionService.createLoanPayment(command));
            }

            this.dialogRef.close(true);

          } catch (err: HttpErrorResponse | any) {
            const errorCode = (err as { error: { errorCode?: string } })?.error?.errorCode;
            const key = errorCode ? errorCode : 'transactions.serverError';
            this.errorMessage.set(key);
          }
        }
      }
    }
  );
}
