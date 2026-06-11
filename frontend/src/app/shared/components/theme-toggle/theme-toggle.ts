import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { ThemeService } from '@core/services/theme.service';
import { TranslocoDirective } from '@jsverse/transloco';

@Component({
  selector: 'app-theme-toggle',
  imports: [TranslocoDirective],
  templateUrl: './theme-toggle.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './theme-toggle.css',
})
export class ThemeToggle {
  protected readonly themeService = inject(ThemeService);

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }

  isDark(): boolean {
    return this.themeService.theme() === 'dark';
  }
}
