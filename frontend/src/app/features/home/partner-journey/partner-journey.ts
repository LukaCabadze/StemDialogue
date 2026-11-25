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
  viewChild,
} from '@angular/core';

import { ViewportScroll } from '../../../core/browser/viewport-scroll.service';
import { LanguageService } from '../../../core/i18n/language.service';
import { FundingDto, PartnerDto } from '../../../core/models';
import { displayDomain } from '../../../core/util/external-url';
import { partnerLogoSrc } from '../../../shared/ui/partner-card/partner-logos';
import { journeyLayout } from './partner-journey.path';

interface Stop {
  readonly partner: PartnerDto;
  readonly roleLabel: string;
  readonly logo: string | null;
  readonly domain: string;
  /** Node position in pixels inside the track. */
  readonly x: number;
  readonly y: number;
  /** Which side of the node the big name sits on. */
  readonly side: 'left' | 'right';
}

/** The line is drawn up to this point of the viewport (0 = top, 1 = bottom). */
const READ_LINE = 0.62;

/**
 * Scroll story for the funder and partner universities, modelled on two26.studio's services line:
 * a thick, smooth blue curve with loops draws itself down the page as you scroll, and each
 * partner's logo, big name and caption pop in when the line reaches it.
 */
@Component({
  selector: 'sd-partner-journey',
  templateUrl: './partner-journey.html',
  styleUrl: './partner-journey.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.is-static]': 'scroll.reducedMotion()',
  },
})
export class PartnerJourney {
  protected readonly scroll = inject(ViewportScroll);
  protected readonly text = inject(LanguageService).text;

  readonly funding = input.required<FundingDto>();

  private readonly track = viewChild.required<ElementRef<HTMLElement>>('track');
  private readonly line = viewChild<ElementRef<SVGPathElement>>('line');

  /** Track width in px (measured), so the path is built in real pixels with no distortion. */
  private readonly width = signal(1200);
  /** Samples of (length, y) along the path, used to draw the line exactly down to the read line. */
  private samples: readonly { length: number; y: number }[] = [];
  private totalLength = 1;

  protected readonly isNarrow = computed(() => this.width() < 720);

  private readonly partners = computed(() => {
    const funding = this.funding();
    const copy = this.text().partners;
    return [
      ...funding.funders.map((partner) => ({ partner, roleLabel: copy.funder })),
      ...funding.partners.map((partner) => ({ partner, roleLabel: copy.universities })),
      ...funding.supporters.map((partner) => ({ partner, roleLabel: copy.supporters })),
    ];
  });

  /** The two26-style curve scaled to the track, with one node per partner. */
  private readonly layout = computed(() => journeyLayout(this.partners().length, this.width(), this.isNarrow()));

  protected readonly stops = computed<readonly Stop[]>(() => {
    const width = this.width();
    const nodes = this.layout().nodes;
    return this.partners().map(({ partner, roleLabel }, index) => {
      const node = nodes[index]!;
      return {
        partner,
        roleLabel,
        logo: partnerLogoSrc(partner),
        domain: displayDomain(partner.websiteUrl),
        x: node.x,
        y: node.y,
        // The big name goes on the roomier side of the node.
        side: node.x > width / 2 ? 'left' : 'right',
      };
    });
  });

  protected readonly height = computed(() => this.layout().height);
  protected readonly stroke = computed(() => this.layout().stroke);
  protected readonly viewBox = computed(() => `0 0 ${this.width()} ${this.height()}`);
  protected readonly path = computed(() => this.layout().d);

  /** Vertical position of the read line inside the track, in px. */
  private readonly reach = signal(0);

  protected readonly drawn = computed(() => {
    if (this.scroll.reducedMotion()) {
      return 1;
    }
    const reach = this.reach();
    const samples = this.samples;
    if (samples.length === 0 || reach <= 0) {
      return 0;
    }
    const hit = samples.find((sample) => sample.y >= reach);
    return hit ? hit.length / this.totalLength : 1;
  });

  constructor() {
    afterNextRender(() => {
      const element = this.track().nativeElement;
      const observer = new ResizeObserver(() => this.width.set(element.clientWidth || 1200));
      observer.observe(element);
      this.width.set(element.clientWidth || 1200);
    });

    // Re-sample the path whenever its shape changes.
    effect(() => {
      this.path();
      const line = this.line()?.nativeElement;
      if (!line) {
        return;
      }
      queueMicrotask(() => this.sample(line));
    });

    // Follow the page scroll.
    effect(() => {
      this.scroll.y();
      const viewport = this.scroll.height();
      const element = this.track().nativeElement;
      const top = element.getBoundingClientRect().top;
      this.reach.set(viewport * READ_LINE - top);
    });
  }

  protected reached(stop: Stop): boolean {
    return this.scroll.reducedMotion() || this.reach() >= stop.y - 20;
  }

  private sample(line: SVGPathElement): void {
    const total = line.getTotalLength();
    if (!total) {
      return;
    }
    const points: { length: number; y: number }[] = [];
    let maxY = -Infinity;
    const count = 400;
    for (let index = 0; index <= count; index++) {
      const length = (total * index) / count;
      // Keep y monotonic so a short upward wiggle never makes the line jump back.
      maxY = Math.max(maxY, line.getPointAtLength(length).y);
      points.push({ length, y: maxY });
    }
    this.samples = points;
    this.totalLength = total;
    this.reach.update((value) => value + 0.0001);
  }
}
