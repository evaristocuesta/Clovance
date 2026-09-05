import { ComponentType } from '@angular/cdk/portal';
import { Dialog, DialogConfig, DialogRef } from '@angular/cdk/dialog';
import { inject, Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class DialogService {
  private static readonly WIDTH = 'min(640px, calc(100vw - 2rem))';
  private static readonly MAX_WIDTH = 'calc(100vw - 2rem)';

  private readonly dialog = inject(Dialog);

  open<R = unknown, D = unknown, C = unknown>(
    component: ComponentType<C>,
    config: Omit<DialogConfig<D, DialogRef<R, C>>, 'width' | 'maxWidth'> = {},
  ): DialogRef<R, C> {
    return this.dialog.open<R, D, C>(component, {
      ...config,
      width: DialogService.WIDTH,
      maxWidth: DialogService.MAX_WIDTH,
    });
  }
}
