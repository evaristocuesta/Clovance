import { AsyncPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { TranslocoModule } from '@jsverse/transloco';
import { AccountCard } from '../account-card/account-card';
import { AccountService } from '../services/account.service';

@Component({
  selector: 'app-accounts-list',
  imports: [AsyncPipe, TranslocoModule, AccountCard],
  templateUrl: './accounts-list.html',
  styleUrl: './accounts-list.css',
})
export class AccountsList {
  private readonly accountService = inject(AccountService);

  protected readonly accounts$ = this.accountService.getAccounts();

  protected editAccount(accountId: string): void {
    console.log('Edit account', accountId);
  }

  protected deleteAccount(accountId: string): void {
    console.log('Delete account', accountId);
  }
}
