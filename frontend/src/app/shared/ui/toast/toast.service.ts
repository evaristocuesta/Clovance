import { Injectable, inject, signal } from '@angular/core';
import { Overlay, OverlayRef } from '@angular/cdk/overlay';
import { ComponentPortal } from '@angular/cdk/portal';
import { ToastContainer } from './toast-container';
import { ToastMessage, ToastVariant } from './toast.model';

const DEFAULT_DURATIONS: Record<ToastVariant, number> = {
  success: 5000,
  info: 5000,
  error: 8000,
};

/** Keeps toasts rendered above any currently open CDK dialog, regardless of which overlay was created first. */
const TOAST_OVERLAY_Z_INDEX = '1100';

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly overlay = inject(Overlay);
  private overlayRef: OverlayRef | null = null;
  private nextId = 0;

  private readonly timers = new Map<number, ReturnType<typeof setTimeout>>();
  private readonly remainingDurations = new Map<number, number>();
  private readonly scheduledAt = new Map<number, number>();

  readonly toasts = signal<ToastMessage[]>([]);

  success(message: string, duration = DEFAULT_DURATIONS.success): void {
    this.show('success', message, duration);
  }

  error(message: string, duration = DEFAULT_DURATIONS.error): void {
    this.show('error', message, duration);
  }

  info(message: string, duration = DEFAULT_DURATIONS.info): void {
    this.show('info', message, duration);
  }

  dismiss(id: number): void {
    this.clearTimer(id);
    this.toasts.update((toasts) => toasts.filter((toast) => toast.id !== id));
  }

  /** Pauses the auto-dismiss countdown, e.g. while the pointer is hovering the toast. */
  pause(id: number): void {
    const timer = this.timers.get(id);
    if (!timer) {
      return;
    }

    clearTimeout(timer);
    this.timers.delete(id);

    const scheduledAt = this.scheduledAt.get(id) ?? Date.now();
    const remaining = this.remainingDurations.get(id) ?? 0;
    this.remainingDurations.set(id, Math.max(0, remaining - (Date.now() - scheduledAt)));
  }

  /** Resumes the auto-dismiss countdown from whatever time was left when it was paused. */
  resume(id: number): void {
    const remaining = this.remainingDurations.get(id);
    if (remaining === undefined || remaining <= 0) {
      return;
    }

    this.scheduleDismiss(id, remaining);
  }

  private show(variant: ToastVariant, message: string, duration: number): void {
    this.ensureAttached();

    const id = this.nextId++;
    this.toasts.update((toasts) => [...toasts, { id, variant, message }]);

    if (duration > 0) {
      this.remainingDurations.set(id, duration);
      this.scheduleDismiss(id, duration);
    }
  }

  private scheduleDismiss(id: number, duration: number): void {
    this.scheduledAt.set(id, Date.now());
    this.timers.set(
      id,
      setTimeout(() => this.dismiss(id), duration),
    );
  }

  private clearTimer(id: number): void {
    const timer = this.timers.get(id);
    if (timer) {
      clearTimeout(timer);
    }

    this.timers.delete(id);
    this.remainingDurations.delete(id);
    this.scheduledAt.delete(id);
  }

  private ensureAttached(): void {
    if (this.overlayRef) {
      return;
    }

    this.overlayRef = this.overlay.create({
      positionStrategy: this.overlay.position().global().top('1rem').right('1rem'),
      scrollStrategy: this.overlay.scrollStrategies.noop(),
    });

    this.overlayRef.hostElement.style.zIndex = TOAST_OVERLAY_Z_INDEX;
    this.overlayRef.attach(new ComponentPortal(ToastContainer));
  }
}
