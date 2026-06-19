import { Injectable, signal } from '@angular/core';
import { ConfirmDialogOptions } from './confirm-dialog-options';

@Injectable({
  providedIn: 'root'
})
export class ConfirmDialogService {

  readonly isOpen = signal(false);
  readonly options = signal<ConfirmDialogOptions | null>(null);

  private resolver: ((value: boolean) => void) | null = null;

  confirm(options: ConfirmDialogOptions): Promise<boolean> {

    this.options.set({
      confirmText: 'Aceptar',
      cancelText: 'Cancelar',
      danger: false,
      ...options
    });

    this.isOpen.set(true);

    return new Promise<boolean>((resolve) => {
      this.resolver = resolve;
    });
  }

  accept(): void {
    this.close(true);
  }

  cancel(): void {
    this.close(false);
  }

  private close(result: boolean): void {

    this.isOpen.set(false);

    this.resolver?.(result);

    this.resolver = null;

    setTimeout(() => {
      this.options.set(null);
    }, 150);
  }
}