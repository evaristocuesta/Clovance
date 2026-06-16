import { Component, inject, signal, ChangeDetectionStrategy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import { TranslocoService, TranslocoDirective } from '@jsverse/transloco';
import { CommonModule } from '@angular/common';
import { LoginRequest } from '@core/models/auth.models';
import { form, required, email, FormField, FormRoot } from '@angular/forms/signals';
import { firstValueFrom } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { LogoFull } from "@shared/components/logo-full/logo-full";

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, TranslocoDirective, FormField, FormRoot, LogoFull],
  templateUrl: './login.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './login.css',
})
export class Login {
  errorMessage = signal('');
  loginRequest = signal<LoginRequest>({ email: '', password: '' });
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  
  loginForm = form(
    this.loginRequest,
    (schema) => {
      required(schema.email, { message: 'login.emailRequired' });
      email(schema.email, { message: 'login.emailInvalid' });
      required(schema.password, {
        message: 'login.passwordRequired',
      });
    },
    {
      submission: {
        action: async (field) => {
          this.errorMessage.set('');

          try {
            await firstValueFrom(this.authService.login(field().value()));
            void this.router.navigateByUrl('/');
          } catch (err: HttpErrorResponse | any) {
            const errorCode = (err as { error: { errorCode?: string } })?.error?.errorCode;
            const key = errorCode ? errorCode : 'login.serverError';
            this.errorMessage.set(key);
          }
        },
      },
    },
  );
}
