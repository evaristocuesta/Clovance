import { Component, computed, input, output } from '@angular/core';
import { Account } from '../models/account.model';
import { TranslocoDirective } from "@jsverse/transloco";
import { Icon } from "@shared/ui/icon/icon";

@Component({
  selector: 'app-account-card',
  imports: [TranslocoDirective, Icon],
  templateUrl: './account-card.html',
  styleUrl: './account-card.css',
})
export class AccountCard {
  readonly account = input.required<Account>();
  readonly currencySymbolMap = input.required<Record<string, string>>();
  readonly editAccount = output<string>();
  readonly deleteAccount = output<string>();
  readonly restoreAccount = output<string>();

  protected accountInitial = computed(() => {
    const name = this.account().name.trim();

    return name ? name.charAt(0).toUpperCase() : 'A';
  });

  protected currencyLabel = computed(() => 
    this.currencySymbolMap()[this.account().currency.toUpperCase()] ?
    `${this.account().currency.toUpperCase()} (${this.currencySymbolMap()[this.account().currency.toUpperCase()]})` 
    : this.account().currency.toUpperCase());

  protected onEdit(): void {
    this.editAccount.emit(this.account().id);
  }

  protected onDelete(): void {
    this.deleteAccount.emit(this.account().id);
  }

  protected onRestore(): void {
    this.restoreAccount.emit(this.account().id);
  }
}
