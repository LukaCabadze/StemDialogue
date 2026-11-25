import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  afterNextRender,
  computed,
  effect,
  inject,
  input,
  signal,
} from '@angular/core';

import { ViewportScroll } from '../../../core/browser/viewport-scroll.service';
import { GlyphStyle, STATIC_GLYPH, WAVE, glyphStyle, parseReveal, plainText } from './scroll-reveal-text.model';

/** The wave starts when the scroll track's top reaches this point of the viewport (0 = top, 1 = bottom). */
const START = 0.35;
/** Share of the track used for reading; the rest holds the finished text before it scrolls away. */
const READ_SHARE = 0.82;

/**
 * Big statement that "reads itself" as you scroll (two26.studio style). The host is a tall scroll
 * track and the statement stays pinned in view while you scroll through it; letters change from
 * light blue to a bright blue edge to dark ink, and a rotated tag pops onto the marked `{word}`.
 * Visitors who prefer reduced motion get the finished text with no pinning.
 */
@Component({
  selector: 'sd-scroll-reveal-text',
  templateUrl: './scroll-reveal-text.html',
  styleUrl: './scroll-reveal-text.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.is-pinned]': '!scroll.reducedMotion()',
  },
})
export class ScrollRevealText {
  protected readonly scroll = inject(ViewportScroll);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  /** Statement text; wrap one word in braces (`{word}`) to pin the tag to it. */
  readonly text = input.required<string>();
  readonly tag = input<string | null>(null);
  readonly overline = input<string | null>(null);

  protected readonly words = computed(() => parseReveal(this.text()));
  protected readonly plain = computed(() => plainText(this.text()));
  private readonly total = computed(() => this.words().reduce((sum, word) => sum + word.glyphs.length, 0));

  /** 0 … 1 through the reveal. */
  private readonly progress = signal(0);
  private readonly reach = computed(() => this.progress() * (this.total() + WAVE));

  protected readonly styles = computed<readonly GlyphStyle[]>(() => {
    const total = this.total();
    if (this.scroll.reducedMotion()) {
      return Array.from({ length: total }, () => STATIC_GLYPH);
    }
    const reach = this.reach();
    return Array.from({ length: total }, (_, index) => glyphStyle(index, reach));
  });

  /** Index of the tagged word's first letter (the tag pops in when the wave gets there). */
  private readonly tagStart = computed(() => this.words().find((word) => word.tagged)?.glyphs[0]?.index ?? -1);
  protected readonly tagVisible = computed(
    () => this.tagStart() >= 0 && (this.scroll.reducedMotion() || this.reach() > this.tagStart() + 2),
  );

  /** Bumped once the page has laid out, so the first measurement is a real one. */
  private readonly laidOut = signal(0);

  constructor() {
    afterNextRender(() => this.laidOut.update((value) => value + 1));

    effect(() => {
      this.laidOut();
      this.scroll.y();
      const viewport = this.scroll.height();
      const rect = this.host.nativeElement.getBoundingClientRect();
      if (rect.height === 0) {
        return; // Not in the document yet.
      }
      const travelled = viewport * START - rect.top;
      const distance = (rect.height - viewport + viewport * START) * READ_SHARE;
      this.progress.set(Math.min(1, Math.max(0, travelled / Math.max(1, distance))));
    });
  }
}
