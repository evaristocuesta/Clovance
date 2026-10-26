import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { DialogRef, DIALOG_DATA } from '@angular/cdk/dialog';
import { Account } from '@features/accounts/models/account.model';
import { Transaction } from '../models/transaction.model';
import { LoanPayment } from '../models/loan-payment.model';
import { SaveLoanPaymentCommand, TransactionService, UpdateLoanPaymentCommand } from '../services/transaction.service';
import { form, FormField, FormRoot, required, validate } from '@angular/forms/signals';
import { toDateOnlyString } from '@shared/utils/date-utils';
import { HttpErrorResponse } from '@angular/common/http';
import { Icon } from '@shared/ui/icon/icon';
import { TranslocoDirective } from '@jsverse/transloco';
import { firstValueFrom, forkJoin } from 'rxjs';
import { Dialog } from '@shared/ui/dialog/dialog';

export interface LoanPaymentFormData {
  toTransaction?: Transaction;
  fromTransaction?: Transaction;
  accounts: Account[];
}

@Component({
  selector: 'app-loan-payment-form',
  imports: [TranslocoDirective, FormField, FormRoot, Icon, Dialog],
  templateUrl: './loan-payment-form.html',
  styleUrl: './loan-payment-form.css',
})
export class LoanPaymentForm implements OnInit {
  errorMessage = signal('');

  /** The pair being edited. Holds the versions the save is checked against. */
  loanPaymentLegs = signal<{ toTransaction: Transaction; fromTransaction: Transaction } | null>(null);

  fromAccounts = computed(() => this.data?.accounts?.filter(a => 
    (Account.canBeUsedForFromLoanPayment(a) && !a.isDeleted)
    || (a.id === this.loanPaymentLegs()?.fromTransaction.accountId && a.isDeleted)) ?? []);
  
  toAccounts = computed(() => this.data?.accounts?.filter(a => 
    (Account.canBeUsedForToLoanPayment(a) && !a.isDeleted)
    || (a.id === this.loanPaymentLegs()?.toTransaction.accountId && a.isDeleted)) ?? []);

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
      const legs = {
        toTransaction: this.data.toTransaction,
        fromTransaction: this.data.fromTransaction,
      };

      this.loanPaymentLegs.set(legs);
      this.applyLoanPaymentLegs(legs);

      // Re-read both legs so the versions sent on save are the ones the user is looking
      // at, not whatever the in-memory list last held. Without this a stale list makes
      // the save fail permanently: reopening the dialog would reuse the same outdated
      // versions and hit the same 409 again.
      forkJoin({
        toTransaction: this.transactionService.getTransactionById(legs.toTransaction.id),
        fromTransaction: this.transactionService.getTransactionById(legs.fromTransaction.id),
      }).subscribe({
        next: (freshLegs) => {
          this.loanPaymentLegs.set(freshLegs);
          this.applyLoanPaymentLegs(freshLegs);
        },
        error: () => {
          // Fall back to the legs the dialog was opened with; the save is still refused
          // if they turn out to be stale.
        },
      });
    }
  }

  private applyLoanPaymentLegs(legs: { toTransaction: Transaction; fromTransaction: Transaction }): void {
    this.loanPayment.set({
      date: legs.fromTransaction.date,
      description: legs.fromTransaction.description,
      // The list stores the amounts on the negative leg, while the form edits them
      // as positive figures.
      amount: -legs.fromTransaction.amount,
      principalAmount: -(legs.fromTransaction.principalAmount || 0),
      fromAccountId: legs.fromTransaction.accountId,
      toAccountId: legs.toTransaction.accountId
    });
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
            const legs = this.loanPaymentLegs();

            if (legs) {
              const command: UpdateLoanPaymentCommand = {
                date: toDateOnlyString(formValue.date),
                description: formValue.description,
                amount: formValue.amount,
                principalAmount: formValue.principalAmount,
                fromAccountId: formValue.fromAccountId,
                toAccountId: formValue.toAccountId,
                // The URL addresses one leg of the pair, so its version goes in "version"
                // and the other leg's in "relatedVersion". Sending back the versions the
                // form was filled from lets the API refuse a save made from stale data.
                version: legs.toTransaction.version,
                relatedVersion: legs.fromTransaction.version,
              };

              await firstValueFrom(this.transactionService.updateLoanPayment(legs.toTransaction.id, command));
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
