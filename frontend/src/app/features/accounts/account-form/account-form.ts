import { Component, inject, signal } from '@angular/core';
import { Account } from '../models/account.model';
import { form, FormField, FormRoot, maxLength, minLength, required } from '@angular/forms/signals';
import { firstValueFrom } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { AccountService } from '../services/account.service';
import { TranslocoDirective } from '@jsverse/transloco';

@Component({
  selector: 'app-account-form',
  imports: [TranslocoDirective, FormField, FormRoot],
  templateUrl: './account-form.html',
  styleUrl: './account-form.css',
})
export class AccountForm {
  errorMessage = signal('');

  account = signal<Account>({
    id: '',
    name: '',
    currency: ''
  });

  private readonly accountService = inject(AccountService);

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
              await firstValueFrom(this.accountService.updateAccount(field().value()));
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
