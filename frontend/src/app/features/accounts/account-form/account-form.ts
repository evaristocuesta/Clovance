import { Component, inject, OnInit, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Account } from '../models/account.model';
import { form, FormField, FormRoot, maxLength, minLength, required } from '@angular/forms/signals';
import { firstValueFrom, map } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { AccountService } from '../services/account.service';
import { TranslocoDirective } from '@jsverse/transloco';
import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { Icon } from "@shared/ui/icon/icon";
import { Currency } from '../models/currency.model';

@Component({
  selector: 'app-account-form',
  imports: [TranslocoDirective, FormField, FormRoot, Icon],
  templateUrl: './account-form.html',
  styleUrl: './account-form.css',
})
export class AccountForm implements OnInit {
  errorMessage = signal('');
  private readonly priorityCurrencyCodes = ['EUR', 'USD', 'JPY', 'GBP'];

  account = signal<Account>({
    id: '',
    name: '',
    currency: ''
  });

  private readonly accountService = inject(AccountService);

  readonly currencies = toSignal(
    this.accountService.getCurrencies().pipe(
      map((currencies) => this.sortCurrencies(currencies)),
    ),
    { initialValue: [] as Currency[] },
  );

  dialogRef = inject<DialogRef>(DialogRef);
  data = inject<Account>(DIALOG_DATA);

  ngOnInit(): void {
    if (this.data?.id) {
      firstValueFrom(this.accountService.getAccountById(this.data.id)).then((account) => {
        this.account.set(account);
      });
    }
  }

  close() {
    this.dialogRef.close();
  }

  private sortCurrencies(currencies: Currency[]): Currency[] {
    return [...currencies].sort((first, second) => {
      const firstCode = first.code.toUpperCase();
      const secondCode = second.code.toUpperCase();
      const firstPriority = this.priorityCurrencyCodes.indexOf(firstCode);
      const secondPriority = this.priorityCurrencyCodes.indexOf(secondCode);

      if (firstPriority !== -1 && secondPriority !== -1) {
        return firstPriority - secondPriority;
      }

      if (firstPriority !== -1) {
        return -1;
      }

      if (secondPriority !== -1) {
        return 1;
      }

      return first.code.localeCompare(second.code, undefined, { sensitivity: 'base' });
    });
  }

  accountForm = form(
      this.account,
      (schemaPath) => {
        maxLength(schemaPath.name, 100, { message: 'accounts.nameMaxLength' });
        required(schemaPath.name, { message: 'accounts.nameRequired' });
        maxLength(schemaPath.currency, 3, { message: 'accounts.currencyLength' });
        minLength(schemaPath.currency, 3, { message: 'accounts.currencyLength' });
        required(schemaPath.currency, { message: 'accounts.currencyRequired' });
      },
      {
        submission: {
          action: async (field) => {  
            this.errorMessage.set('');
  
            try {
              if (this.data?.id) {
                await firstValueFrom(this.accountService.updateAccount(field().value()));
              } else {
                await firstValueFrom(this.accountService.createAccount(field().value()));
              }
              this.dialogRef.close(true);
            } catch (err: HttpErrorResponse | any) {
              const errorCode = (err as { error: { errorCode?: string } })?.error?.errorCode;
              const key = errorCode ? errorCode : 'accounts.serverError';
              this.errorMessage.set(key);
            }
          },
        },
      },
    );
}
