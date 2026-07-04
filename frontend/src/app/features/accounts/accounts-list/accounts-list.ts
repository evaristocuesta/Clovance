import { AsyncPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { TranslocoModule } from '@jsverse/transloco';
import { AccountCard } from '../account-card/account-card';
import { AccountService } from '../services/account.service';
import { Icon } from "@shared/ui/icon/icon";

@Component({
  selector: 'app-accounts-list',
  imports: [AsyncPipe, TranslocoModule, AccountCard, Icon],
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

  onAdd(): void {
    console.log('Add new account');
  }
}
