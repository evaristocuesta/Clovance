import { Component, inject, signal, ChangeDetectionStrategy, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import { TranslocoDirective } from '@jsverse/transloco';
import { CommonModule } from '@angular/common';
import { ResetPasswordRequest } from '@core/models/auth.models';
import { form, required, minLength, validate, FormField, FormRoot } from '@angular/forms/signals';
import { firstValueFrom } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { LogoFull } from '@shared/components/logo-full/logo-full';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, TranslocoDirective, FormField, FormRoot, LogoFull, RouterLink],
  templateUrl: './reset-password.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './reset-password.css',
})
export class ResetPassword implements OnInit {
  errorMessage = signal('');
  submitted = signal(false);
  linkInvalid = signal(false);

  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  resetPasswordRequest = signal<ResetPasswordRequest & { confirmNewPassword: string }>({
    email: '',
    token: '',
    newPassword: '',
    confirmNewPassword: '',
  });

  resetPasswordForm = form(
    this.resetPasswordRequest,
    (schemaPath) => {
      required(schemaPath.newPassword, { message: 'resetPassword.newPasswordRequired' });
      minLength(schemaPath.newPassword, 12, { message: 'resetPassword.newPasswordMinLength' });

      validate(schemaPath.newPassword, ({ value }) => {
        const password = value();

        if (!/[0-9]/.test(password)) {
          return { kind: 'passwordMissingDigit', message: 'resetPassword.newPasswordMissingDigit' };
        }

        return null;
      });
      validate(schemaPath.newPassword, ({ value }) => {
        const password = value();

        if (!/[a-z]/.test(password)) {
          return { kind: 'passwordMissingLowercase', message: 'resetPassword.newPasswordMissingLowercase' };
        }

        return null;
      });
      validate(schemaPath.newPassword, ({ value }) => {
        const password = value();

        if (!/[A-Z]/.test(password)) {
          return { kind: 'passwordMissingUppercase', message: 'resetPassword.newPasswordMissingUppercase' };
        }

        return null;
      });
      validate(schemaPath.newPassword, ({ value }) => {
        const password = value();

        if (!/[^a-zA-Z0-9]/.test(password)) {
          return {
            kind: 'passwordMissingNonAlphanumeric',
            message: 'resetPassword.newPasswordMissingNonAlphanumeric',
          };
        }

        return null;
      });

      required(schemaPath.confirmNewPassword, { message: 'resetPassword.confirmNewPasswordRequired' });
      validate(schemaPath.confirmNewPassword, ({ value, valueOf }) => {
        const confirmPassword = value();
        const password = valueOf(schemaPath.newPassword);

        if (confirmPassword !== password) {
          return { kind: 'passwordMismatch', message: 'resetPassword.passwordMismatch' };
        }

        return null;
      });
    },
    {
      submission: {
        action: async (field) => {
          this.errorMessage.set('');

          try {
            await firstValueFrom(this.authService.resetPassword(field().value()));
            this.submitted.set(true);
          } catch (err: HttpErrorResponse | any) {
            const errorCode = (err as { error: { errorCode?: string } })?.error?.errorCode;
            const key = errorCode ? errorCode : 'resetPassword.serverError';
            this.errorMessage.set(key);
          }
        },
      },
    },
  );

  ngOnInit(): void {
    const email = this.route.snapshot.queryParamMap.get('email');
    const token = this.route.snapshot.queryParamMap.get('token');

    if (!email || !token) {
      this.linkInvalid.set(true);
      return;
    }

    this.resetPasswordRequest.update((current) => ({ ...current, email, token }));
  }
}