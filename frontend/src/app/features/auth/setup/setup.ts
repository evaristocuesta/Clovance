import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { email, form, FormField, FormRoot, minLength, required, validate } from '@angular/forms/signals';
import { Router } from '@angular/router';
import { SetupRequest } from '@core/models/auth.models';
import { AuthService } from '@core/services/auth.service';
import { TranslocoDirective, TranslocoService } from '@jsverse/transloco';
import { firstValueFrom } from 'rxjs/internal/firstValueFrom';

@Component({
  selector: 'app-setup',
  imports: [CommonModule, TranslocoDirective, FormField, FormRoot],
  templateUrl: './setup.html',
  styleUrl: './setup.css',
})
export class Setup {
  errorMessage = signal('');
  
  setupRequest = signal<SetupRequest>({
    email: '', 
    password: '',
    confirmPassword: ''
  });

  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly translocoService = inject(TranslocoService);

  setupForm = form(
    this.setupRequest,
    (schemaPath) => {
      required(schemaPath.email, { message: this.translocoService.translate('setup.emailRequired') });
      email(schemaPath.email, { message: this.translocoService.translate('setup.emailInvalid') });
      required(schemaPath.password, {
        message: this.translocoService.translate('setup.passwordRequired'),
      });
      minLength(schemaPath.password, 12, {message: this.translocoService.translate('setup.passwordMinLength')});
      validate(schemaPath.password, ({ value }) => {
        const password = value();

        if (!/[0-9]/.test(password)) {
          return {
            kind: 'passwordMissingDigit',
            message: this.translocoService.translate('setup.passwordMissingDigit'),
          };
        }

        return null;
      });
      validate(schemaPath.password, ({ value }) => {
        const password = value();

        if (!/[a-z]/.test(password)) {
          return {
            kind: 'passwordMissingLowercase',
            message: this.translocoService.translate('setup.passwordMissingLowercase'),
          };
        }

        return null;
      });
      validate(schemaPath.password, ({ value }) => {
        const password = value();

        if (!/[A-Z]/.test(password)) {
          return {
            kind: 'passwordMissingUppercase',
            message: this.translocoService.translate('setup.passwordMissingUppercase'),
          };
        }

        return null;
      });
      validate(schemaPath.password, ({ value }) => {
        const password = value();

        if (!/[^a-zA-Z0-9]/.test(password)) {
          return {
            kind: 'passwordMissingNonAlphanumeric',
            message: this.translocoService.translate('setup.passwordMissingNonAlphanumeric'),
          };
        }

        return null;
      });
      required(schemaPath.confirmPassword, {
        message: this.translocoService.translate('setup.confirmPasswordRequired'),
      });
      validate(schemaPath.confirmPassword, ({value, valueOf}) => {
        const confirmPassword = value();
        const password = valueOf(schemaPath.password);

        if (confirmPassword !== password) {
          return {
            kind: 'passwordMismatch',
            message: this.translocoService.translate('setup.passwordMismatch'),
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
            await firstValueFrom(this.authService.setup(field().value()));
            void this.router.navigateByUrl('/auth/login');
          } catch (err: HttpErrorResponse | any) {
            const errorCode = (err as { error: { errorCode?: string } })?.error?.errorCode;
            const key = errorCode ? errorCode : 'setup.serverError';
            this.errorMessage.set(this.translocoService.translate(key));
          }
        },
      },
    },
  );
}
