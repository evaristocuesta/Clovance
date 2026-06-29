import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { form, FormField, FormRoot, minLength, required, validate } from '@angular/forms/signals';
import { AuthService } from '@core/services/auth.service';
import { TranslocoDirective } from '@jsverse/transloco';
import { firstValueFrom } from 'rxjs/internal/firstValueFrom';

@Component({
  selector: 'app-change-password',
  imports: [TranslocoDirective, FormField, FormRoot],
  templateUrl: './change-password.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './change-password.css',
})
export class ChangePassword {
  errorMessage = signal('');

  changePasswordRequest = signal({
    currentPassword: '',
    newPassword: '',
    confirmNewPassword: ''
  });

  private readonly authService = inject(AuthService);

  changePasswordForm = form(
    this.changePasswordRequest,
    (schemaPath) => {
      required(schemaPath.currentPassword, {
        message: 'changePassword.currentPasswordRequired',
      });
      required(schemaPath.newPassword, {
        message: 'changePassword.newPasswordRequired',
      });
      minLength(schemaPath.newPassword, 12, {message: 'changePassword.newPasswordMinLength'});
      validate(schemaPath.newPassword, ({ value }) => {
        const password = value();

        if (!/[0-9]/.test(password)) {
          return {
            kind: 'passwordMissingDigit',
            message: 'changePassword.newPasswordMissingDigit',
          };
        }

        return null;
      });
      validate(schemaPath.newPassword, ({ value }) => {
        const password = value();

        if (!/[a-z]/.test(password)) {
          return {
            kind: 'passwordMissingLowercase',
            message: 'changePassword.newPasswordMissingLowercase',
          };
        }

        return null;
      });
      validate(schemaPath.newPassword, ({ value }) => {
        const password = value();

        if (!/[A-Z]/.test(password)) {
          return {
            kind: 'passwordMissingUppercase',
            message: 'changePassword.newPasswordMissingUppercase',
          };
        }

        return null;
      });
      validate(schemaPath.newPassword, ({ value }) => {
        const password = value();

        if (!/[^a-zA-Z0-9]/.test(password)) {
          return {
            kind: 'passwordMissingNonAlphanumeric',
            message: 'changePassword.newPasswordMissingNonAlphanumeric',
          };
        }

        return null;
      });
      required(schemaPath.confirmNewPassword, {
        message: 'changePassword.confirmNewPasswordRequired',
      });
      validate(schemaPath.confirmNewPassword, ({value, valueOf}) => {
        const confirmPassword = value();
        const password = valueOf(schemaPath.newPassword);

        if (confirmPassword !== password) {
          return {
            kind: 'passwordMismatch',
            message: 'changePassword.passwordMismatch',
          };
        }

        return null;
      });
    }, 
    {
      submission: {
        action: async (field) => {  
          this.errorMessage.set('');

          try {
            await firstValueFrom(this.authService.changePassword(field().value()));
          } catch (err: HttpErrorResponse | any) {
            const errorCode = (err as { error: { errorCode?: string } })?.error?.errorCode;
            const key = errorCode ? errorCode : 'changePassword.serverError';
            this.errorMessage.set(key);
          }
        },
      },
    }
  );
} 
