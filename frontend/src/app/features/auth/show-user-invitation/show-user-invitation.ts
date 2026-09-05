import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { CreateInvitationResult } from '@core/models/auth.models';
import { TranslocoModule } from '@jsverse/transloco';
import { Dialog } from '@shared/ui/dialog/dialog';

@Component({
  selector: 'app-show-user-invitation',
  imports: [TranslocoModule, DatePipe, Dialog],
  templateUrl: './show-user-invitation.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './show-user-invitation.css',
})
export class ShowUserInvitation {
  dialogRef = inject<DialogRef>(DialogRef);
  data = inject<CreateInvitationResult>(DIALOG_DATA);

  close(): void {
    this.dialogRef.close();
  }
}
