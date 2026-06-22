import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { CreateInvitationResult } from '@core/models/auth.models';
import { TranslocoModule } from '@jsverse/transloco';
import { Icon } from "@shared/ui/icon/icon";

@Component({
  selector: 'app-show-user-invitation',
  imports: [Icon, TranslocoModule, DatePipe],
  templateUrl: './show-user-invitation.html',
  styleUrl: './show-user-invitation.css',
})
export class ShowUserInvitation {
  dialogRef = inject<DialogRef>(DialogRef);
  data = inject<CreateInvitationResult>(DIALOG_DATA);

  close() {
    this.dialogRef.close();
  }
}
