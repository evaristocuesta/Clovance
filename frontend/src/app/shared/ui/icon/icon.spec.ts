import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component, signal } from '@angular/core';

import { Icon } from './icon';
import { IconName } from './icon-name';
import { ICONS } from './icon.registry';

@Component({
  selector: 'app-icon-host',
  template: `<app-icon [name]="name()" class="h-6 w-6" />`,
  imports: [Icon],
})
class Host {
  // The app runs without zone.js, so change detection is signal-driven: only a signal
  // marks this view for check. A plain property would never reach the `app-icon` input.
  readonly name = signal<IconName>('close');
}

// The `name` input is typed as `IconName`, so only markup from the static registry ever
// reaches the component. The markup is parsed with DOMParser and copied in as real SVG
// nodes, which keeps it out of `[innerHTML]` and out of `bypassSecurityTrustHtml`
// entirely: Angular's HTML sanitizer allows only HTML tags and strips every SVG element,
// so an HTML binding could not render these icons at all. These tests pin down what must
// still hold: every icon renders, keeps the attributes that draw it, and re-renders when
// the name changes.
describe('Icon', () => {
  let fixture: ComponentFixture<Host>;
  let host: Host;

  // `app-icon` is the component host element; the rendered `<svg>` is its child and
  // carries the drawing attributes and shapes.
  const iconEl = (): SVGSVGElement => {
    const element = (fixture.nativeElement as HTMLElement).querySelector('app-icon svg');
    if (!element) {
      throw new Error(
        `icon rendered no svg element: ${(fixture.nativeElement as HTMLElement).innerHTML}`,
      );
    }
    return element as SVGSVGElement;
  };

  const render = (name: IconName) => {
    host.name.set(name);
    fixture.detectChanges();
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    fixture = TestBed.createComponent(Host);
    host = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('renders the svg markup for the requested icon', () => {
    expect(iconEl().querySelector('path')).not.toBeNull();
  });

  it('keeps the drawing attributes of the icon markup', () => {
    const path = iconEl().querySelector('path') as SVGPathElement;

    expect(path.getAttribute('d')).toBe('M6 18 17.94 6M18 18 6.06 6');
    expect(path.getAttribute('stroke')).toBe('currentColor');
    expect(path.getAttribute('stroke-width')).toBe('2');
    expect(path.getAttribute('stroke-linecap')).toBe('round');
    expect(path.getAttribute('stroke-linejoin')).toBe('round');
    expect(iconEl().getAttribute('viewBox')).toBe('0 0 24 24');
    expect(iconEl().getAttribute('fill')).toBe('none');
    expect(iconEl().getAttribute('aria-hidden')).toBe('true');
  });

  it('applies the class input to the rendered svg', () => {
    expect(iconEl().getAttribute('class')).toBe('h-6 w-6');
  });

  // Some icons are authored on a 20x20, 16x16 or 122.88x118.19 grid, so the viewBox must
  // follow the registry rather than being hardcoded in the template.
  it.each([
    ['close', '0 0 24 24'],
    ['moon', '0 0 20 20'],
    ['savings', '0 0 16 16'],
    ['loan', '0 0 122.88 118.19'],
  ] as [IconName, string][])(
    'renders %s with the viewBox authored in the registry',
    (name, viewBox) => {
      render(name);

      expect(iconEl().getAttribute('viewBox')).toBe(viewBox);
    },
  );

  // The flags are the only icons carrying inline `style` and `transform`, so they are the
  // markup most likely to be rewritten by a sanitizer and the ones worth pinning.
  it.each([['flag-es'], ['flag-en']] as IconName[][])(
    'keeps the inline styles and transforms of %s renderable',
    (name) => {
      render(name);

      const group = iconEl().querySelector('g');

      expect(group, name).not.toBeNull();
      expect(group?.getAttribute('transform'), name).not.toBeNull();
      expect(group?.getAttribute('style'), name).not.toBeNull();
    },
  );

  // Several registry entries omit `xmlns`; their shapes must still be SVG-namespaced or the
  // browser renders nothing.
  it('creates shapes in the SVG namespace even when the entry has no xmlns', () => {
    for (const name of Object.keys(ICONS) as IconName[]) {
      render(name);

      for (const shape of Array.from(iconEl().children)) {
        expect(shape.namespaceURI, name).toBe('http://www.w3.org/2000/svg');
      }
    }
  });

  it('keeps clip-rule on the icons that use it', () => {
    render('trash-bin');

    expect(iconEl().querySelector('path')?.getAttribute('clip-rule')).toBe('evenodd');
  });

  it('replaces the previous icon when the name changes', () => {
    render('trash-bin');

    expect(iconEl().getAttribute('fill')).toBe('currentColor');
    expect(iconEl().querySelectorAll('path').length).toBeGreaterThan(0);

    render('close');

    expect(iconEl().getAttribute('fill')).toBe('none');
    expect(iconEl().querySelectorAll('path').length).toBeGreaterThan(0);
  });

  it('renders every icon in the registry', () => {
    for (const name of Object.keys(ICONS) as IconName[]) {
      render(name);

      expect(iconEl().querySelectorAll('path').length, name).toBeGreaterThan(0);
    }
  });
});
