import { ChangeDetectionStrategy, Component } from '@angular/core';
import { TranslocoDirective } from '@jsverse/transloco';

@Component({
  selector: 'app-transactions',
  imports: [TranslocoDirective],
  templateUrl: './transactions.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './transactions.css',
})
export class Transactions {}
