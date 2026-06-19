import { ChangeDetectionStrategy, Component, Input, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ICONS } from './icon.registry';
import { IconName } from './icon-name';
import { DomSanitizer } from '@angular/platform-browser';

@Component({
  selector: 'app-icon',
  imports: [CommonModule],
  templateUrl: './icon.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Icon {

  private sanitizer = inject(DomSanitizer);

  @Input({ required: true })
  name!: IconName;

  @Input()
  class = 'w-5 h-5';

  protected svg = computed(() => this.sanitizer.bypassSecurityTrustHtml(ICONS[this.name]));
}
