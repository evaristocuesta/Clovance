import { Component, inject } from '@angular/core';
import { TranslocoService, TranslocoDirective } from '@jsverse/transloco';

@Component({
  selector: 'app-language-selection',
  imports: [TranslocoDirective],
  templateUrl: './language-selection.html',
  styleUrl: './language-selection.css',
})
export class LanguageSelection {
  private readonly translocoService = inject(TranslocoService);

  changeLanguage(lang: string) {
    // Close the dropdown before changing language to avoid positioning issues
    const dropdown = document.getElementById('language-dropdown');
    if (dropdown) {
      dropdown.classList.add('hidden');
    }
    // Save language preference to localStorage
    localStorage.setItem('transloco-lang', lang);
    this.translocoService.setActiveLang(lang);
  }

  getCurrentLanguage(): string {
    return this.translocoService.getActiveLang();
  }
}
