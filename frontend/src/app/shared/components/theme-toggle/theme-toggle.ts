import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { ThemeService } from '@core/services/theme.service';
import { TranslocoDirective } from '@jsverse/transloco';
import { Icon } from "@shared/ui/icon/icon";

@Component({
  selector: 'app-theme-toggle',
  imports: [TranslocoDirective, Icon],
  templateUrl: './theme-toggle.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
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
