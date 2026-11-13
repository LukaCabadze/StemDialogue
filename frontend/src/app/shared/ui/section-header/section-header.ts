import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { Icon } from '../icon/icon';

export type SectionHeaderTone = 'light' | 'dark';

/** "0N · overline", H2 title, optional lead, and an optional "see all →" link on the right. */
@Component({
  selector: 'sd-section-header',
  imports: [RouterLink, Icon],
  templateUrl: './section-header.html',
  styleUrl: './section-header.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.tone-dark]': "tone() === 'dark'",
  },
})
export class SectionHeader {
  readonly index = input<string | null>(null);
  readonly overline = input.required<string>();
  readonly title = input.required<string>();
  readonly lead = input<string | null>(null);
  readonly linkLabel = input<string | null>(null);
  readonly linkPath = input<string | null>(null);
  readonly linkQuery = input<Readonly<Record<string, string>> | null>(null);
  readonly headingId = input<string | null>(null);
  readonly tone = input<SectionHeaderTone>('light');
}
