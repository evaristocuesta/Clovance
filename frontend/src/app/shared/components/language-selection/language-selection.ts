import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { TranslocoService, TranslocoDirective } from '@jsverse/transloco';
import Dropdown from 'flowbite/lib/esm/components/dropdown';

@Component({
  selector: 'app-language-selection',
  imports: [TranslocoDirective],
  templateUrl: './language-selection.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
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

    // Re-initialize the dropdown to ensure it works correctly after changing language
    const $trigger = document.querySelector('[data-dropdown-toggle="language-dropdown"]') as HTMLElement;
    const $menu = document.getElementById('language-dropdown') as HTMLElement;

    if ($trigger && $menu) {
      const dropdown = new Dropdown($menu, $trigger);
      dropdown.hide();
    }
    
    // Save language preference to localStorage
    localStorage.setItem('transloco-lang', lang);
    this.translocoService.setActiveLang(lang);
  }

  getCurrentLanguage(): string {
    return this.translocoService.getActiveLang();
  }
}
