import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { LanguageService } from '../../../core/i18n/language.service';
import { FeaturedDto, MediaDto } from '../../../core/models';
import { ActionLink } from '../../../shared/ui/action-link/action-link';
import { CoverArt } from '../../../shared/ui/cover-art/cover-art';
import { Icon } from '../../../shared/ui/icon/icon';
import { Pill } from '../../../shared/ui/pill/pill';
import { SdDatePipe } from '../../../shared/pipes/sd-date.pipe';

interface FeaturedView {
  readonly kindLabel: string;
  readonly categoryLabel: string;
  readonly date: string;
  readonly title: string;
  readonly summary: string | null;
  readonly path: string;
  readonly cover: MediaDto | null;
  readonly registrationUrl: string | null;
}

/** Wide highlight card: illustration on the left, category + date, title, text and actions. */
@Component({
  selector: 'sd-home-featured',
  imports: [RouterLink, ActionLink, CoverArt, Icon, Pill, SdDatePipe],
  templateUrl: './home-featured.html',
  styleUrl: './home-featured.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeFeatured {
  private readonly languages = inject(LanguageService);
  protected readonly text = this.languages.text;
  protected readonly language = this.languages.language;

  readonly featured = input.required<FeaturedDto>();

  protected readonly view = computed<FeaturedView | null>(() => {
    const featured = this.featured();
    const copy = this.text().home;
    if (featured.news) {
      const news = featured.news;
      return {
        kindLabel: copy.featuredNews,
        categoryLabel: news.categoryLabel,
        date: news.publishedAt,
        title: news.title,
        summary: news.summary,
        path: news.path,
        cover: news.cover,
        registrationUrl: null,
      };
    }
    if (featured.event) {
      const event = featured.event;
      return {
        kindLabel: copy.featuredEvent,
        categoryLabel: event.typeLabel,
        date: event.startsAt,
        title: event.title,
        summary: event.summary,
        path: event.path,
        cover: event.cover,
        registrationUrl: event.isUpcoming ? event.registrationUrl : null,
      };
    }
    return null;
  });
}
