import { ChangeDetectionStrategy, Component } from '@angular/core';
import { TranslocoDirective } from '@jsverse/transloco';

@Component({
  selector: 'app-accounts',
  imports: [TranslocoDirective],
  templateUrl: './accounts.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './accounts.css',
})
export class Accounts {}
