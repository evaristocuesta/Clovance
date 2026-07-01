import { ChangeDetectionStrategy, Component } from '@angular/core';
import { TranslocoDirective } from '@jsverse/transloco';

@Component({
  selector: 'app-summary',
  imports: [TranslocoDirective],
  templateUrl: './summary.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './summary.css',
})
export class Summary {}
