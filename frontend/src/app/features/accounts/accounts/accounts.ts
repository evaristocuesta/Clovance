import { ChangeDetectionStrategy, Component } from '@angular/core';
import { TranslocoDirective } from '@jsverse/transloco';
import { AccountsList } from "../accounts-list/accounts-list";

@Component({
  selector: 'app-accounts',
  imports: [TranslocoDirective, AccountsList],
  templateUrl: './accounts.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './accounts.css',
})
export class Accounts {}
