import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { email, form, FormField, FormRoot, required } from '@angular/forms/signals';
import { CreateInvitationCommand, CreateInvitationResult } from '@core/models/auth.models';
import { AuthService } from '@core/services/auth.service';
import { Icon } from "@shared/ui/icon/icon";
import { firstValueFrom } from 'rxjs/internal/firstValueFrom';
import { DialogRef } from '@angular/cdk/dialog';
import { TranslocoModule } from '@jsverse/transloco';

@Component({
  selector: 'app-invite-user',
  imports: [Icon, FormRoot, FormField, TranslocoModule],
  templateUrl: './invite-user.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './invite-user.css',
})
export class InviteUser {
  readonly authService = inject(AuthService);
  readonly dialogRef = inject<DialogRef<CreateInvitationResult>>(DialogRef<CreateInvitationResult>);

  errorMessage = signal('');

  userInvitationCommand = signal<CreateInvitationCommand>({
    email: '',
    isAdmin: false
  });

  sendInvitationForm = form(
    this.userInvitationCommand,
    (schemaPath) => {
      required(schemaPath.email, { message: 'users.emailRequired' });
      email(schemaPath.email, { message: 'users.emailInvalid' });
    },
    {
      submission: {
        action: async (field) => {  
          this.errorMessage.set('');
          
          try {
            var result = await firstValueFrom(this.authService.sendInvitation(field().value()));
            this.dialogRef.close(result);
          } catch (err: HttpErrorResponse | any) {
            const errorCode = (err as { error: { errorCode?: string } })?.error?.errorCode;
            const key = errorCode ? errorCode : 'login.serverError';
            this.errorMessage.set(key);
          }
        },
      },
    }
  );

  close() {
    this.dialogRef.close();
  }
}
