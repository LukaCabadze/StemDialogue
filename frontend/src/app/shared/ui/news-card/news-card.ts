import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { LanguageService } from '../../../core/i18n/language.service';
import { NewsCardDto } from '../../../core/models';
import { CoverVariant, coverVariantFor } from '../../../core/util/cover-variant';
import { SdDatePipe } from '../../pipes/sd-date.pipe';
import { CoverArt } from '../cover-art/cover-art';
import { Pill } from '../pill/pill';

/** News card: 200px cover (photo or illustrated placeholder), category pill, date, title, summary. */
@Component({
  selector: 'sd-news-card',
  imports: [RouterLink, CoverArt, Pill, SdDatePipe],
  templateUrl: './news-card.html',
  styleUrl: './news-card.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NewsCard {
  protected readonly language = inject(LanguageService).language;

  readonly news = input.required<NewsCardDto>();
  /** Overrides the slug-derived placeholder colour (e.g. to alternate in a row). */
  readonly variant = input<CoverVariant | null>(null);
  readonly headingLevel = input<2 | 3>(3);

  protected readonly coverVariant = computed(() => this.variant() ?? coverVariantFor(this.news().slug));
  /** Summaries are pink-tinted, everything else blue — as in the design. */
  protected readonly categoryTone = computed(() => (this.news().category === 'EventSummary' ? 'pink' : 'blue'));
}
