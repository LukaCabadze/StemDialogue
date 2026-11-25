import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  afterNextRender,
  computed,
  inject,
  signal,
} from '@angular/core';

import { buildLabFrame } from './lab-frame.geometry';

let nextId = 0;

interface Size {
  readonly width: number;
  readonly height: number;
}

/**
 * Wraps page content in an animated chemistry set: a glass tube from an Erlenmeyer flask runs over
 * the top of the content and down into a round-bottom flask. When the frame comes into view the
 * tube draws itself, blue liquid starts flowing through it, the flasks fill with sloshing liquid
 * and bubbles rise. Hidden on narrow screens; static for visitors who prefer reduced motion.
 *
 * Usage: `<sd-lab-frame> …page content… </sd-lab-frame>`
 */
@Component({
  selector: 'sd-lab-frame',
  templateUrl: './lab-frame.html',
  styleUrl: './lab-frame.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.is-visible]': 'visible()',
  },
})
export class LabFrame {
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  private readonly size = signal<Size>({ width: 0, height: 0 }, {
    equal: (a, b) => a.width === b.width && a.height === b.height,
  });

  protected readonly visible = signal(false);
  protected readonly frame = computed(() => buildLabFrame(this.size().width, this.size().height));

  private readonly id = `sd-lab-${nextId++}`;
  protected readonly clip = {
    leftId: `${this.id}-left`,
    rightId: `${this.id}-right`,
    left: `url(#${this.id}-left)`,
    right: `url(#${this.id}-right)`,
  };

  constructor() {
    const destroyRef = inject(DestroyRef);

    afterNextRender(() => {
      const element = this.host.nativeElement;
      const measure = () => this.size.set({ width: element.clientWidth, height: element.clientHeight });
      const resize = new ResizeObserver(measure);
      resize.observe(element);
      measure();

      const reveal = new IntersectionObserver(
        (entries) => {
          if (entries.some((entry) => entry.isIntersecting)) {
            this.visible.set(true);
            reveal.disconnect();
          }
        },
        { threshold: 0.05 },
      );
      reveal.observe(element);

      destroyRef.onDestroy(() => {
        resize.disconnect();
        reveal.disconnect();
      });
    });
  }
}
