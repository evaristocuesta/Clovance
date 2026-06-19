import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  HostListener,
  ViewChild,
  inject,
  effect,
  afterNextRender
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { ConfirmDialogService } from './confirm-dialog.service';
import { Icon } from "../icon/icon";
import { IconName } from '../icon/icon-name';

@Component({
  selector: 'app-confirm-dialog',
  imports: [CommonModule, Icon],
  templateUrl: './confirm-dialog.html',
  styleUrl: './confirm-dialog.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ConfirmDialog {

  protected readonly dialog = inject(ConfirmDialogService);

  @ViewChild('modalPanel')
  protected modalPanel?: ElementRef<HTMLDivElement>;

  constructor() {

    effect(() => {

      if (!this.dialog.isOpen()) {
        return;
      }

      setTimeout(() => {
        this.focusFirstElement();
      });

    });
  }

  protected get confirmIcon(): IconName | null {
    return this.dialog.options()?.confirmIcon ?? null;
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {

    if (this.dialog.isOpen()) {
      this.dialog.cancel();
    }

  }

  protected onBackdropClick(): void {
    this.dialog.cancel();
  }

  protected stopPropagation(event: MouseEvent): void {
    event.stopPropagation();
  }

   @HostListener('document:keydown.tab', ['$event'])
  trapFocus(event: Event): void {

    const keyboardEvent = event as KeyboardEvent;

    if (!this.dialog.isOpen()) {
      return;
    }

    const container = this.modalPanel?.nativeElement;

    if (!container) {
      return;
    }

    const focusable = container.querySelectorAll<HTMLElement>(
      `
      button,
      [href],
      input,
      select,
      textarea,
      [tabindex]:not([tabindex="-1"])
      `
    );

    if (!focusable.length) {
      return;
    }

    const first = focusable[0];
    const last = focusable[focusable.length - 1];

    if (
      keyboardEvent.shiftKey &&
      document.activeElement === first
    ) {
      keyboardEvent.preventDefault();
      last.focus();
    }

    if (
      !keyboardEvent.shiftKey &&
      document.activeElement === last
    ) {
      keyboardEvent.preventDefault();
      first.focus();
    }
  }

  private focusFirstElement(): void {

    const container = this.modalPanel?.nativeElement;

    if (!container) {
      return;
    }

    const firstFocusable =
      container.querySelector<HTMLElement>(
        'button,[href],[tabindex]:not([tabindex="-1"])'
      );

    firstFocusable?.focus();
  }
}
