import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';

import { ViewportScroll } from '../../../core/browser/viewport-scroll.service';

interface Orbit {
  readonly angle: number;
  readonly tone: 'blue' | 'pink';
  /** Electron start position (radians) and travel direction on this orbit. */
  readonly start: number;
  readonly direction: 1 | -1;
}

interface ElectronPosition {
  readonly x: number;
  readonly y: number;
}

const RX = 96;
const RY = 34;
/** Radians of electron travel per pixel scrolled. */
const SPEED = 0.0065;

/**
 * The project's atom mark drawn in code: three symmetric orbits (60° apart) around a nucleus.
 * The electrons travel along their orbits and the whole atom turns slowly as the page scrolls.
 */
@Component({
  selector: 'sd-atom-mark',
  templateUrl: './atom-mark.html',
  styleUrl: './atom-mark.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[style.width.px]': 'size()',
    '[style.height.px]': 'size()',
  },
})
export class AtomMark {
  private readonly scroll = inject(ViewportScroll);

  readonly size = input(220);
  /** Accessible name; decorative when empty. */
  readonly label = input('');

  protected readonly orbits: readonly Orbit[] = [
    { angle: 0, tone: 'pink', start: 0.4, direction: 1 },
    { angle: 60, tone: 'blue', start: 2.6, direction: -1 },
    { angle: 120, tone: 'pink', start: 4.4, direction: 1 },
  ];
  protected readonly rx = RX;
  protected readonly ry = RY;

  private readonly travel = computed(() => (this.scroll.reducedMotion() ? 0 : this.scroll.y() * SPEED));

  /** Slow overall rotation tied to scroll (degrees). */
  protected readonly spin = computed(() => this.travel() * 9);

  protected readonly electrons = computed<readonly ElectronPosition[]>(() =>
    this.orbits.map((orbit) => {
      const t = orbit.start + orbit.direction * this.travel();
      return { x: RX * Math.cos(t), y: RY * Math.sin(t) };
    }),
  );
}
