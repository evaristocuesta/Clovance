import { Injectable, signal } from '@angular/core';

export type Theme = 'light' | 'dark';

@Injectable({
  providedIn: 'root',
})
export class ThemeService {  
  private readonly THEME_KEY = 'color-theme';

  theme = signal<Theme>(this.getInitialTheme());
  private themeToggleBtn: HTMLElement | null = null;

  constructor() {
    // Initialize the theme
    this.applyTheme(this.theme());
  }

  /**
   * Get the initial theme from localStorage o system preferencies
   */
  private getInitialTheme(): Theme {

    // First, try to get from localStorage
    const storedTheme = globalThis.localStorage?.getItem(this.THEME_KEY) as Theme | null;
    if (storedTheme === 'light' || storedTheme === 'dark') {
      return storedTheme;
    }

    // If theme is not saved, use system preferencies
    if (globalThis.matchMedia?.('(prefers-color-scheme: dark)').matches) {
      return 'dark';
    }

    return 'light';
  }

  /**
   * Apply theme to HTML document
   */
  private applyTheme(theme: Theme): void {
    const htmlElement = globalThis.document?.documentElement;
    if (!htmlElement) return;

    if (theme === 'dark') {
      htmlElement.classList.add('dark');
    } else {
      htmlElement.classList.remove('dark');
    }

    // Save theme in localStorage
    globalThis.localStorage?.setItem(this.THEME_KEY, theme);

    // Update signal
    this.theme.set(theme);
  }

  /**
   * Swicth between light and dark mode
   */
  toggleTheme(): void {
    const newTheme = this.theme() === 'light' ? 'dark' : 'light';
    this.applyTheme(newTheme);
  }

}
