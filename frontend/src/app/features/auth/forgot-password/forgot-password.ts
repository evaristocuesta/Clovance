import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import { TranslocoDirective } from '@jsverse/transloco';
import { CommonModule } from '@angular/common';
import { ForgotPasswordRequest } from '@core/models/auth.models';
import { form, required, email, FormField, FormRoot } from '@angular/forms/signals';
import { firstValueFrom } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { LogoFull } from '@shared/components/logo-full/logo-full';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, TranslocoDirective, FormField, FormRoot, LogoFull, RouterLink],
  templateUrl: './forgot-password.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './forgot-password.css',
})
export class ForgotPassword {
  errorMessage = signal('');
  submitted = signal(false);
  forgotPasswordRequest = signal<ForgotPasswordRequest>({ email: '' });
  private readonly authService = inject(AuthService);
  forgotPasswordForm = form(
    this.forgotPasswordRequest,
    (schema) => {
      required(schema.email, { message: 'forgotPassword.emailRequired' });
      email(schema.email, { message: 'forgotPassword.emailInvalid' });
    },
    {
      submission: {
        action: async (field) => {
          this.errorMessage.set('');

          try {
            await firstValueFrom(this.authService.forgotPassword(field().value()));
            this.submitted.set(true);
          } catch (err: HttpErrorResponse | any) {
            const errorCode = (err as { error: { errorCode?: string } })?.error?.errorCode;
            const key = errorCode ? errorCode : 'forgotPassword.serverError';
            this.errorMessage.set(key);
          }
        },
      },
    },
  );
}