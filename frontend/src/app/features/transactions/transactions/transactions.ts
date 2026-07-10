import { ChangeDetectionStrategy, Component } from '@angular/core';
import { TranslocoDirective } from '@jsverse/transloco';
import { TransactionList } from "../transaction-list/transaction-list";

@Component({
  selector: 'app-transactions',
  imports: [TranslocoDirective, TransactionList],
  templateUrl: './transactions.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './transactions.css',
})
export class Transactions {}
