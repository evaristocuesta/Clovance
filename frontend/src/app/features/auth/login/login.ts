import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import { TranslocoService, TranslocoDirective } from '@jsverse/transloco';
import { CommonModule } from '@angular/common';
import { LoginRequest } from '@core/models/auth.models';
import { form, required, email, FormField, FormRoot } from '@angular/forms/signals';
import { firstValueFrom } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, TranslocoDirective, FormField, FormRoot],
  templateUrl: './login.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './login.css',
})
export class Login {
  errorMessage = signal('');
  loginRequest = signal<LoginRequest>({ email: '', password: '' });
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly translocoService = inject(TranslocoService);

  loginForm = form(
    this.loginRequest,
    (schema) => {
      required(schema.email, { message: this.translocoService.translate('login.emailRequired') });
      email(schema.email, { message: this.translocoService.translate('login.emailInvalid') });
      required(schema.password, {
        message: this.translocoService.translate('login.passwordRequired'),
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
            const status = (err as { status?: number })?.status;
            const errorCode = (err as { error: { errorCode?: string } })?.error?.errorCode;
            const key = status === 401 && errorCode ? errorCode : 'login.serverError';
            this.errorMessage.set(this.translocoService.translate(key));
          }
        },
      },
    },
  );
}
