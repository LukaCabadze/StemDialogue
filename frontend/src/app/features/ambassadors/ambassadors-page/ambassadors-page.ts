import { ChangeDetectionStrategy, Component, computed, effect, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';

import { AmbassadorsApi } from '../../../core/api/ambassadors.api';
import { LanguageService } from '../../../core/i18n/language.service';
import { AmbassadorsDto } from '../../../core/models';
import { pagedResource } from '../../../core/paging/paged-resource';
import { ProjectContext } from '../../../core/project/project-context.service';
import { SeoService } from '../../../core/seo/seo.service';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { ErrorState } from '../../../shared/ui/error-state/error-state';
import { LoadMore } from '../../../shared/ui/load-more/load-more';
import { LoadingState } from '../../../shared/ui/loading-state/loading-state';
import { PageHero } from '../../../shared/ui/page-hero/page-hero';
import { RichText } from '../../../shared/ui/rich-text/rich-text';
import { ScrollRevealText } from '../../../shared/ui/scroll-reveal-text/scroll-reveal-text';
import { SectionHeader } from '../../../shared/ui/section-header/section-header';
import { ActivityCard } from '../activity-card/activity-card';

const PAGE_SIZE = 9;

/** STEM Ambassadors programme page: scroll-read manifesto plus the archive of school initiatives. */
@Component({
  selector: 'sd-ambassadors-page',
  imports: [
    ActivityCard,
    EmptyState,
    ErrorState,
    LoadMore,
    LoadingState,
    PageHero,
    RichText,
    ScrollRevealText,
    SectionHeader,
  ],
  templateUrl: './ambassadors-page.html',
  styleUrl: './ambassadors-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AmbassadorsPage {
  private readonly api = inject(AmbassadorsApi);
  private readonly seo = inject(SeoService);
  private readonly languages = inject(LanguageService);
  private readonly project = inject(ProjectContext);
  protected readonly text = this.languages.text;

  protected readonly overview = rxResource({
    params: () => ({ slug: this.project.slug(), language: this.languages.language() }),
    stream: ({ params }) => this.api.overview(params.slug),
  });

  protected readonly activities = pagedResource({
    query: () => ({ slug: this.project.slug(), language: this.languages.language() }),
    load: (query, page) => this.api.activities(query.slug, { page, pageSize: PAGE_SIZE }),
  });

  protected readonly data = computed<AmbassadorsDto | null>(() => (this.overview.hasValue() ? this.overview.value() : null));
  protected readonly title = computed(() => this.data()?.page?.title ?? this.text().ambassadors.title);
  protected readonly crumbs = computed(() => [
    { label: this.text().nav.home, path: this.project.rootPath() },
    { label: this.title() },
  ]);

  constructor() {
    effect(() => {
      const page = this.data()?.page;
      if (page) {
        this.seo.apply(page.seo);
      } else {
        this.seo.apply({ title: this.title(), canonicalPath: this.project.path('stem-ambassadors') });
      }
    });
  }
}
