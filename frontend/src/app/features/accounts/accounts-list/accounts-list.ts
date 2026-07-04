import { AsyncPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { TranslocoModule } from '@jsverse/transloco';
import { AccountCard } from '../account-card/account-card';
import { AccountService } from '../services/account.service';
import { Icon } from "@shared/ui/icon/icon";
import { Dialog } from '@angular/cdk/dialog';
import { AccountForm } from '../account-form/account-form';

@Component({
  selector: 'app-accounts-list',
  imports: [AsyncPipe, TranslocoModule, AccountCard, Icon],
  templateUrl: './accounts-list.html',
  styleUrl: './accounts-list.css',
})
export class AccountsList {
  private readonly accountService = inject(AccountService);
  readonly dialog = inject(Dialog);

  protected readonly accounts$ = this.accountService.getAccounts();

  protected editAccount(accountId: string): void {
    console.log('Edit account', accountId);
  }

  protected deleteAccount(accountId: string): void {
    console.log('Delete account', accountId);
  }

  onAdd(): void {
    const dialogRef = this.dialog.open<void>(AccountForm, {
      width: '640px',
      height: 'auto',
    });

    dialogRef.closed.subscribe((result) => {
      if (result) {
        
      }
    });
  }
}
