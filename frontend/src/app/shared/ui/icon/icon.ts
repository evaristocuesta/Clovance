import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
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

  readonly name = input.required<IconName>();

  readonly className = input('w-5 h-5', { alias: 'class' });

  protected svg = computed(() => this.sanitizer.bypassSecurityTrustHtml(ICONS[this.name()]));
}
