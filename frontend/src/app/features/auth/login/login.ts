import { AfterViewInit, Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import { TranslocoService } from '@jsverse/transloco';
import { initFlowbite } from 'flowbite';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { LoginRequest } from '@core/models/auth.models';
import { ThemeToggle } from '@shared/components/theme-toggle/theme-toggle';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ThemeToggle],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login implements AfterViewInit {

  errorMessage = signal('');
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly translocoService = inject(TranslocoService);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required]],
    password: ['', [Validators.required]],
  });

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

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage.set('');
    const { email, password } = this.form.getRawValue();

    const loginRequest: LoginRequest = {
      email: email,
      password: password,
    };

    this.authService.login(loginRequest).subscribe({
      next: () => void this.router.navigateByUrl('/'),
      error: () => {
        this.errorMessage.set(this.translocoService.translate('login.invalidCredentials'));
      },
    });
  }  
}
