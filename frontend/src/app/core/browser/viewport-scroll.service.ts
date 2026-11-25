import { DOCUMENT } from '@angular/common';
import { DestroyRef, Injectable, inject, signal } from '@angular/core';

/**
 * Window scroll position and size as signals, updated at most once per animation frame.
 * Scroll-driven visuals read these instead of adding their own listeners.
 */
@Injectable({ providedIn: 'root' })
export class ViewportScroll {
  private readonly window = inject(DOCUMENT).defaultView;
  private frame: number | null = null;

  readonly y = signal(0);
  readonly height = signal(800);
  readonly width = signal(1280);
  /** Visitors who asked the OS for less motion get static visuals. */
  readonly reducedMotion = signal(false);

  constructor() {
    const win = this.window;
    if (!win) {
      return;
    }

    const media = win.matchMedia('(prefers-reduced-motion: reduce)');
    this.reducedMotion.set(media.matches);
    const onMotionChange = (event: MediaQueryListEvent) => this.reducedMotion.set(event.matches);
    media.addEventListener('change', onMotionChange);

    const measure = () => {
      this.frame = null;
      this.y.set(win.scrollY);
      this.height.set(win.innerHeight);
      this.width.set(win.innerWidth);
    };
    const schedule = () => {
      if (this.frame === null) {
        this.frame = win.requestAnimationFrame(measure);
      }
    };

    measure();
    win.addEventListener('scroll', schedule, { passive: true });
    win.addEventListener('resize', schedule, { passive: true });

    inject(DestroyRef).onDestroy(() => {
      win.removeEventListener('scroll', schedule);
      win.removeEventListener('resize', schedule);
      media.removeEventListener('change', onMotionChange);
      if (this.frame !== null) {
        win.cancelAnimationFrame(this.frame);
      }
    });
  }
}
