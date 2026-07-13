import { Component, input, output } from '@angular/core';
import { Account } from '@features/accounts/models/account.model';
import { TranslocoDirective } from '@jsverse/transloco';
import { TransactionFilters } from '../services/transaction.service';
import { form, FormField, FormRoot, validate } from '@angular/forms/signals';
import { signal } from '@angular/core';
import { Icon } from "@shared/ui/icon/icon";

interface TransactionFiltersFormValue {
  dateFrom: string;
  dateTo: string;
  accountId: string;
  description: string;
}

@Component({
  selector: 'app-transactions-filter',
  imports: [TranslocoDirective, FormField, FormRoot, Icon],
  templateUrl: './transactions-filter.html',
  styleUrl: './transactions-filter.css',
})
export class TransactionsFilter {
  readonly accounts = input<Account[]>([]);
  readonly applyFilters = output<TransactionFilters>();

  private readonly filterState = signal<TransactionFiltersFormValue>({
    dateFrom: '',
    dateTo: '',
    accountId: '',
    description: '',
  });

  protected readonly filterForm = form(
    this.filterState, 
    (schemaPath) => {
      validate(schemaPath.dateFrom, ({ value, valueOf }) => {
        const dateFrom = value();
        const dateTo = valueOf(schemaPath.dateTo);

        if ((dateFrom && !dateTo) || (!dateFrom && dateTo)) {
          return {
            kind: 'dateToAndDateFromRequired',
            message: 'transactions.filters.dateToAndDateFromRequired',
          };
        }

        if (dateFrom && dateTo && new Date(dateFrom) > new Date(dateTo)) {
          return {
            kind: 'invalidDateRange',
            message: 'transactions.filters.invalidDateRange',
          };
        }

        return null;
      });

      validate(schemaPath.dateTo, ({ value, valueOf }) => {
        const dateTo = value();
        const dateFrom = valueOf(schemaPath.dateFrom);

        if ((dateFrom && !dateTo) || (!dateFrom && dateTo)) {
          return {
            kind: 'dateToAndDateFromRequired',
            message: 'transactions.filters.dateToAndDateFromRequired',
          };
        }

        if (dateFrom && dateTo && new Date(dateFrom) > new Date(dateTo)) {
          return {
            kind: 'invalidDateRange',
            message: 'transactions.filters.invalidDateRange',
          };
        }

        return null;
      });
    }, 
    {
      submission: {
        action: async (field) => {
          const formValue = field().value();

          this.applyFilters.emit({
            dateFrom: formValue.dateFrom || null,
            dateTo: formValue.dateTo || null,
            accountId: formValue.accountId || null,
            description: formValue.description.trim() || null,
          });
        },
      },
    }
  );

  protected getDateRangeErrorMessage(): string | null {
    const dateFromError = this.filterForm.dateFrom().errors()[0]?.message;
    if (dateFromError) {
      return dateFromError;
    }

    const dateToError = this.filterForm.dateTo().errors()[0]?.message;
    return dateToError ?? null;
  }

  onReset(): void {
    this.filterState.set({
      dateFrom: '',
      dateTo: '',
      accountId: '',
      description: '',
    });

    this.applyFilters.emit({
      dateFrom: null,
      dateTo: null,
      accountId: null,
      description: null,
    });
  }
}
