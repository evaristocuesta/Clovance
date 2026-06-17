import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { email, form, FormField, FormRoot, maxLength, minLength, required, validate } from '@angular/forms/signals';
import { Router } from '@angular/router';
import { RegisterWithInvitationRequest } from '@core/models/auth.models';
import { AuthService } from '@core/services/auth.service';
import { TranslocoDirective } from '@jsverse/transloco';
import { LogoFull } from '@shared/components/logo-full/logo-full';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-register-user',
  imports: [CommonModule, TranslocoDirective, FormField, FormRoot, LogoFull],
  templateUrl: './register-user.html',
  styleUrl: './register-user.css',
})
export class RegisterUser {

  errorMessage = signal('');
    
    registerRequest = signal<RegisterWithInvitationRequest>({
      firstName: '',
      lastName: '',
      email: '', 
      password: '',
      confirmPassword: '',
      token: ''
    });
  
    private readonly authService = inject(AuthService);
    private readonly router = inject(Router);

  registerForm = form(
    this.registerRequest,
    (schemaPath) => {
      maxLength(schemaPath.firstName, 100, { message: 'setup.firstNameMaxLength' }); 
      required(schemaPath.firstName, { message: 'setup.firstNameRequired' });
      maxLength(schemaPath.lastName, 100, { message: 'setup.lastNameMaxLength' });
      required(schemaPath.lastName, { message: 'setup.lastNameRequired' });
      required(schemaPath.email, { message: 'setup.emailRequired' });
      email(schemaPath.email, { message: 'setup.emailInvalid' });
      required(schemaPath.password, {
        message: 'setup.passwordRequired',
      });
      minLength(schemaPath.password, 12, {message: 'setup.passwordMinLength'});
      validate(schemaPath.password, ({ value }) => {
        const password = value();

        if (!/[0-9]/.test(password)) {
          return {
            kind: 'passwordMissingDigit',
            message: 'setup.passwordMissingDigit',
          };
        }

        return null;
      });
      validate(schemaPath.password, ({ value }) => {
        const password = value();

        if (!/[a-z]/.test(password)) {
          return {
            kind: 'passwordMissingLowercase',
            message: 'setup.passwordMissingLowercase',
          };
        }

        return null;
      });
      validate(schemaPath.password, ({ value }) => {
        const password = value();

        if (!/[A-Z]/.test(password)) {
          return {
            kind: 'passwordMissingUppercase',
            message: 'setup.passwordMissingUppercase',
          };
        }

        return null;
      });
      validate(schemaPath.password, ({ value }) => {
        const password = value();

        if (!/[^a-zA-Z0-9]/.test(password)) {
          return {
            kind: 'passwordMissingNonAlphanumeric',
            message: 'setup.passwordMissingNonAlphanumeric',
          };
        }

        return null;
      });
      required(schemaPath.confirmPassword, {
        message: 'setup.confirmPasswordRequired',
      });
      validate(schemaPath.confirmPassword, ({value, valueOf}) => {
        const confirmPassword = value();
        const password = valueOf(schemaPath.password);

        if (confirmPassword !== password) {
          return {
            kind: 'passwordMismatch',
            message: 'setup.passwordMismatch',
          };
        }

        return null;
      });
      required(schemaPath.token, {
        message: 'setup.tokenRequired',
      });
    },
    {
      submission: {
        action: async (field) => {
          this.errorMessage.set('');

          try {
            await firstValueFrom(this.authService.registerWithInvitation(field().value()));
            void this.router.navigateByUrl('/auth/login');
          } catch (err: HttpErrorResponse | any) {
            const errorCode = (err as { error: { errorCode?: string } })?.error?.errorCode;
            const key = errorCode ? errorCode : 'setup.serverError';
            this.errorMessage.set(key);
          }
        },
      },
    },
  );
}
