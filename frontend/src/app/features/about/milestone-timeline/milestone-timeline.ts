import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  effect,
  inject,
  input,
  linkedSignal,
  signal,
  untracked,
  viewChildren,
} from '@angular/core';

import { ViewportScroll } from '../../../core/browser/viewport-scroll.service';
import { LanguageService } from '../../../core/i18n/language.service';
import { MilestoneDto } from '../../../core/models';

/** A stage becomes active when its top passes this point of the viewport while scrolling. */
const SPY_LINE = 0.45;
/** After a click the scroll spy waits until the smooth scroll has settled. */
const CLICK_LOCK_MS = 900;
/** Centre of the marker relative to the top of its stage, in px (see .marker in the SCSS). */
const MARKER_CENTER = 15;

/**
 * Vertical project timeline. Clicking a stage (card or circle) scrolls it into view and makes it
 * the active one (red circle, highlighted card); while scrolling, the active stage follows the page.
 * A red progress line runs down to the active stage. Done stages are blue, the stage the project is
 * in now keeps a red ring.
 */
@Component({
  selector: 'sd-milestone-timeline',
  templateUrl: './milestone-timeline.html',
  styleUrl: './milestone-timeline.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MilestoneTimeline {
  private readonly scroll = inject(ViewportScroll);
  protected readonly text = inject(LanguageService).text;

  readonly milestones = input.required<readonly MilestoneDto[]>();

  private readonly stages = viewChildren<ElementRef<HTMLElement>>('stage');

  /** Selected stage; starts on the stage the project is in now. */
  protected readonly active = linkedSignal(() =>
    Math.max(
      0,
      this.milestones().findIndex((milestone) => milestone.state === 'Current'),
    ),
  );

  /** Height of the red progress line, in px. */
  protected readonly progress = signal(0);

  private lockedUntil = 0;

  constructor() {
    // Scroll spy: the active stage follows the page while the user scrolls.
    effect(() => {
      this.scroll.y();
      const viewport = this.scroll.height();
      const stages = this.stages();
      if (stages.length === 0) {
        return;
      }
      untracked(() => {
        const now = performance.now();
        if (now < this.lockedUntil) {
          // Still the click's smooth scroll: keep the clicked stage while it is moving.
          this.lockedUntil = Math.max(this.lockedUntil, now + 200);
          return;
        }
        if (stages[0]!.nativeElement.getBoundingClientRect().height === 0) {
          return; // Not laid out yet.
        }
        const line = viewport * SPY_LINE;
        let index = -1;
        stages.forEach((stage, position) => {
          if (stage.nativeElement.getBoundingClientRect().top <= line) {
            index = position;
          }
        });
        // At the very bottom of the page the last stages can never reach the line.
        const last = stages[stages.length - 1]!.nativeElement.getBoundingClientRect();
        const doc = document.documentElement;
        if (window.scrollY + viewport >= doc.scrollHeight - 4 && last.top < viewport) {
          index = stages.length - 1;
        }
        if (index >= 0) {
          this.active.set(index);
        }
      });
    });

    // Progress line follows the active stage (and re-measures on resize).
    effect(() => {
      const index = this.active();
      this.scroll.width();
      const stage = this.stages()[index];
      if (stage) {
        this.progress.set(stage.nativeElement.offsetTop + MARKER_CENTER);
      }
    });
  }

  protected select(index: number): void {
    const stage = this.stages()[index]?.nativeElement;
    this.active.set(index);
    this.lockedUntil = performance.now() + CLICK_LOCK_MS;
    stage?.scrollIntoView({ behavior: this.scroll.reducedMotion() ? 'auto' : 'smooth', block: 'center' });
  }
}
