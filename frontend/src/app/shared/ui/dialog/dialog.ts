import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { DialogRef } from '@angular/cdk/dialog';
import { TranslocoService } from '@jsverse/transloco';
import { Icon } from '../icon/icon';

@Component({
  selector: 'app-dialog',
  imports: [Icon],
  templateUrl: './dialog.html',
  styleUrl: './dialog.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Dialog {
  readonly title = input.required<string>();
  private readonly dialogRef = inject(DialogRef);
  private readonly translocoService = inject(TranslocoService);

  close(): void {
    this.dialogRef.close();
  }

  closeLabel(): string {
    return this.translocoService.translate('common.closeDialog');
  }
}
