import { ChangeDetectionStrategy, Component, inject, input, output } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { Icon } from '../icon/icon';
import { IconName } from '../icon/icon-name';
import { ToastMessage, ToastVariant } from './toast.model';

const ICON_BY_VARIANT: Record<ToastVariant, IconName> = {
  success: 'check-circle',
  error: 'exclamation-circle',
  info: 'info-circle',
};

@Component({
  selector: 'app-toast',
  imports: [Icon],
  templateUrl: './toast.html',
  styleUrl: './toast.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Toast {
  readonly toast = input.required<ToastMessage>();

  readonly dismissed = output<number>();
  readonly paused = output<number>();
  readonly resumed = output<number>();

  private readonly translocoService = inject(TranslocoService);

  protected get icon(): IconName {
    return ICON_BY_VARIANT[this.toast().variant];
  }

  protected get role(): 'alert' | 'status' {
    return this.toast().variant === 'error' ? 'alert' : 'status';
  }

  protected closeLabel(): string {
    return this.translocoService.translate('common.closeNotification');
  }

  dismiss(): void {
    this.dismissed.emit(this.toast().id);
  }

  pause(): void {
    this.paused.emit(this.toast().id);
  }

  resume(): void {
    this.resumed.emit(this.toast().id);
  }
}
