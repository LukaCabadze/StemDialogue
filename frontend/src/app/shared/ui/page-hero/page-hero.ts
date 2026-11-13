import { ChangeDetectionStrategy, Component, input } from '@angular/core';

import { Breadcrumbs, Crumb } from '../breadcrumbs/breadcrumbs';

/**
 * Listing-page header: soft gradient band with breadcrumbs, H1 and lead.
 * Projected content (filters, tabs) renders under the lead.
 */
@Component({
  selector: 'sd-page-hero',
  imports: [Breadcrumbs],
  templateUrl: './page-hero.html',
  styleUrl: './page-hero.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PageHero {
  readonly crumbs = input.required<readonly Crumb[]>();
  readonly title = input.required<string>();
  readonly lead = input<string | null>(null);
}
