import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  ElementRef,
  inject,
  input,
  viewChild,
} from '@angular/core';
import { ICONS } from './icon.registry';
import { IconName } from './icon-name';

@Component({
  selector: 'app-icon',
  templateUrl: './icon.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Icon {
  readonly name = input.required<IconName>();

  readonly className = input('w-5 h-5', { alias: 'class' });

  /** The `<svg>` this component renders; the registry shapes are drawn inside it. */
  private readonly svg = viewChild.required<ElementRef<SVGSVGElement>>('svg');

  /**
   * The registry markup is parsed into real SVG nodes instead of being handed to an
   * `[innerHTML]` binding. Angular's HTML sanitizer only allows HTML elements (its
   * `VALID_ELEMENTS` list has no `svg`, `path` or `g`), so an HTML binding would strip
   * every icon down to an empty element unless it were wrapped in
   * `bypassSecurityTrustHtml`. It is parsed as HTML (not `image/svg+xml`) because the HTML
   * parser puts `<svg>` content in the SVG namespace even when the entry has no `xmlns`;
   * the XML parser would leave such nodes namespace-less and the browser would not paint
   * them. Parsing runs outside the sanitizer, and the nodes are
   * copied in with `importNode` rather than re-serialized into a string, so there is no
   * injection point to exploit.
   *
   * Only the static strings keyed by the typed `IconName` input ever reach this code. Do
   * not reintroduce `bypassSecurityTrustHtml` here, and do not widen this to untrusted
   * markup: if a caller ever needs dynamic markup, sanitize it before parsing.
   */
  private readonly parsed = computed(
    () =>
      new DOMParser().parseFromString(ICONS[this.name()], 'text/html').body.querySelector('svg')!,
  );

  /** Some icons are authored on a 20x20, 16x16 or 256x256 grid, so this comes from the registry. */
  protected readonly viewBox = computed(() => this.parsed().getAttribute('viewBox') ?? '0 0 24 24');

  /** Mirrors the registry's own `fill`, so `fill="none"` stroke icons stay unfilled. */
  protected readonly fill = computed(() => this.parsed().getAttribute('fill') ?? 'none');

  constructor() {
    // `parsed()` is a signal, so the effect re-runs whenever `name` changes and the
    // previous shapes are swapped. `replaceChildren` is atomic: it leaves the `<svg>`'s
    // own attributes (class, viewBox, fill), which are regular template bindings, intact.
    effect(() => {
      this.svg().nativeElement.replaceChildren(
        ...Array.from(this.parsed().children, (shape) => document.importNode(shape, true)),
      );
    });
  }
}
