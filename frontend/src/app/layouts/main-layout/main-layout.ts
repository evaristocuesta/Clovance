import { Component, ChangeDetectionStrategy, AfterViewInit, inject } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import { TranslocoDirective, TranslocoService } from '@jsverse/transloco';
import { LanguageSelection } from '@shared/components/language-selection/language-selection';
import { ThemeToggle } from '@shared/components/theme-toggle/theme-toggle';
import { initFlowbite } from 'flowbite';
import { Logo } from "@shared/components/logo/logo";
import { LogoFull } from "@shared/components/logo-full/logo-full";

@Component({
  selector: 'app-main-layout',
  imports: [RouterOutlet, ThemeToggle, LanguageSelection, TranslocoDirective, Logo, LogoFull, RouterLink],
  templateUrl: './main-layout.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './main-layout.css',
})
export class MainLayout implements AfterViewInit {

  private readonly translocoService = inject(TranslocoService);
  private readonly router = inject(Router);

  readonly authService = inject(AuthService);
  
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

  logout() {
    this.authService.logout().subscribe({
      next: () => {
        void this.router.navigateByUrl('/auth/login');
      },
      error: () => {
        void this.router.navigateByUrl('/auth/login');
      },
    });
  }
}
