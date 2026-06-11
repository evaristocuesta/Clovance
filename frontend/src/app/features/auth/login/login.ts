import { AfterViewInit, Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import { TranslocoService, TranslocoDirective } from '@jsverse/transloco';
import { initFlowbite } from 'flowbite';
import { CommonModule } from '@angular/common';
import { LoginRequest } from '@core/models/auth.models';
import { ThemeToggle } from '@shared/components/theme-toggle/theme-toggle';
import { LanguageSelection } from '@shared/components/language-selection/language-selection';
import { form, required, email, FormField, FormRoot } from '@angular/forms/signals';
import { firstValueFrom } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, TranslocoDirective, ThemeToggle, LanguageSelection, FormField, FormRoot],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login implements AfterViewInit {

  errorMessage = signal('');
  loginRequest = signal<LoginRequest>({ email: '', password: '' });
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly translocoService = inject(TranslocoService);

  loginForm = form(
    this.loginRequest, 
    (schema) => {
      required(schema.email, {message: this.translocoService.translate('login.emailRequired')});
      email(schema.email, {message: this.translocoService.translate('login.emailInvalid')});
      required(schema.password, {message: this.translocoService.translate('login.passwordRequired')});
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
            const key = status === 401 && errorCode
              ? errorCode
              : 'login.serverError';
            this.errorMessage.set(this.translocoService.translate(key));
          }
        },
      }
    }
  );

  constructor() {
    // Load saved language preference
    const savedLang = localStorage.getItem('transloco-lang');
    if (savedLang && (savedLang === 'es' || savedLang === 'en')) {
      this.translocoService.setActiveLang(savedLang);
    }
  }

  ngAfterViewInit(): void {
// Initialize Flowbite after view is rendered
    setTimeout(() => {
      initFlowbite();
    }, 100);
  }
}
