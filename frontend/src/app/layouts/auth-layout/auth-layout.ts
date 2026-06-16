import { Component, ChangeDetectionStrategy, AfterViewInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TranslocoService } from '@jsverse/transloco';
import { LanguageSelection } from '@shared/components/language-selection/language-selection';
import { ThemeToggle } from '@shared/components/theme-toggle/theme-toggle';
import { initFlowbite } from 'flowbite';
import { LogoFull } from "@shared/components/logo-full/logo-full";
import { Logo } from "@shared/components/logo/logo";

@Component({
  selector: 'app-auth-layout',
  imports: [RouterOutlet, ThemeToggle, LanguageSelection, LogoFull, Logo],
  templateUrl: './auth-layout.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './auth-layout.css',
})
export class AuthLayout implements AfterViewInit {

  private readonly translocoService = inject(TranslocoService);

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
