import { Component, computed, inject, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { TranslocoModule, TranslocoService } from '@jsverse/transloco';
import { AccountCard } from '../account-card/account-card';
import { AccountService } from '../services/account.service';
import { Icon } from "@shared/ui/icon/icon";
import { Dialog, DialogRef } from '@angular/cdk/dialog';
import { AccountForm } from '../account-form/account-form';
import { filter, map, startWith, switchMap, take, tap } from 'rxjs';
import { Account } from '../models/account.model';
import { ConfirmDialog } from '@shared/ui/confirm-dialog/confirm-dialog';

@Component({
  selector: 'app-accounts-list',
  imports: [TranslocoModule, AccountCard, Icon],
  templateUrl: './accounts-list.html',
  styleUrl: './accounts-list.css',
})
export class AccountsList {
  private readonly accountService = inject(AccountService);
  readonly translocoService = inject(TranslocoService);
  readonly dialog = inject(Dialog);
  private readonly refreshTick = signal(0);

  readonly currencies = toSignal(this.accountService.getCurrencies(), { initialValue: [] });

  readonly currencySymbolMap = computed(() =>
    Object.fromEntries(this.currencies().map((c) => [c.code.toUpperCase(), c.symbol]))
  );

  protected readonly accounts = toSignal(
    toObservable(this.refreshTick).pipe(
      switchMap(() => this.accountService.getAccounts().pipe(
        map((accounts) => [...accounts].sort((a, b) => a.name.localeCompare(b.name))),
      )),
      startWith(null as Account[] | null),
    ),
  );

  private refreshAccounts(): void {
    this.refreshTick.update((value) => value + 1);
  }

  private refreshOnDialogSuccess(dialogRef: DialogRef<boolean>): void {
    dialogRef.closed
      .pipe(
        take(1),
        filter((result): result is true => result === true),
        tap(() => this.refreshAccounts()),
      )
      .subscribe();
  }

  protected onEdit(accountId: string): void {
    const dialogRef = this.dialog.open<boolean>(AccountForm, {
      width: '640px',
      height: 'auto',
      data: { id: accountId }
    });

    this.refreshOnDialogSuccess(dialogRef);
  }

  protected onDelete(accountId: string): void {

    const name = this.accounts()?.find((account) => account.id === accountId)?.name || '';

    const dialogRef = this.dialog.open(ConfirmDialog, {
          width: '640px',
          height: 'auto',
          data: {
            title: this.translocoService.translate('accounts.confirmDeleteTitle'),
            message: this.translocoService.translate('accounts.confirmDeleteMessage', { name }),
            confirmText: this.translocoService.translate('accounts.delete'),
            confirmIcon: 'trash-bin',
            cancelText: this.translocoService.translate('accounts.cancel'),
            danger: true
          } 
        });
    
        dialogRef.closed.subscribe((confirmed) => {
          if (!confirmed) {
            return;
          }
    
          this.accountService.deleteAccount(accountId).subscribe({
            next: () => {
              this.refreshAccounts();
            },
            error: (error) => {
              console.error('Error deleting account:', error);
            }
          });
        });
  }

  onAdd(): void {
    const dialogRef = this.dialog.open<boolean>(AccountForm, {
      width: '640px',
      height: 'auto',
    });

    this.refreshOnDialogSuccess(dialogRef);
  }
}
