import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Toast } from './toast';
import { ToastService } from './toast.service';

@Component({
  selector: 'app-toast-container',
  imports: [Toast],
  templateUrl: './toast-container.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ToastContainer {
  protected readonly toastService = inject(ToastService);
}
