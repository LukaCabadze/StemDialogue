import { ChangeDetectionStrategy, Component, computed, effect, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';

import { NewsApi } from '../../../core/api/news.api';
import { LanguageService } from '../../../core/i18n/language.service';
import { NewsCategory } from '../../../core/models';
import { pagedResource } from '../../../core/paging/paged-resource';
import { ProjectContext } from '../../../core/project/project-context.service';
import { SeoService } from '../../../core/seo/seo.service';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { ErrorState } from '../../../shared/ui/error-state/error-state';
import { ChipOption, FilterChips } from '../../../shared/ui/filter-chips/filter-chips';
import { LoadMore } from '../../../shared/ui/load-more/load-more';
import { LoadingState } from '../../../shared/ui/loading-state/loading-state';
import { NewsCard } from '../../../shared/ui/news-card/news-card';
import { PageHero } from '../../../shared/ui/page-hero/page-hero';

const ALL = 'all';
const PAGE_SIZE = 9;

/** News listing with category tabs (`?category=EventSummary`). */
@Component({
  selector: 'sd-news-page',
  imports: [EmptyState, ErrorState, FilterChips, LoadMore, LoadingState, NewsCard, PageHero],
  templateUrl: './news-page.html',
  styleUrl: './news-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NewsPage {
  private readonly api = inject(NewsApi);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly seo = inject(SeoService);
  private readonly languages = inject(LanguageService);
  protected readonly project = inject(ProjectContext);
  protected readonly text = this.languages.text;

  /** Query parameter. */
  readonly category = input<string>();

  private readonly categories = rxResource({
    params: () => ({ slug: this.project.slug(), language: this.languages.language() }),
    stream: ({ params }) => this.api.categories(params.slug),
  });

  protected readonly selected = computed(() => this.category() || ALL);

  protected readonly options = computed<readonly ChipOption[]>(() => {
    const categories = this.categories.hasValue() ? this.categories.value() : [];
    const total = categories.reduce((sum, item) => sum + item.count, 0);
    return [
      { value: ALL, label: this.text().news.allCategories, count: categories.length ? total : null },
      ...categories
        .filter((item) => item.count > 0)
        .map((item) => ({ value: item.value, label: item.label, count: item.count })),
    ];
  });

  protected readonly news = pagedResource({
    query: () => ({
      slug: this.project.slug(),
      language: this.languages.language(),
      category: this.selected() === ALL ? undefined : (this.selected() as NewsCategory),
    }),
    load: (query, page) => this.api.list(query.slug, { category: query.category, page, pageSize: PAGE_SIZE }),
  });

  protected readonly crumbs = computed(() => [
    { label: this.text().nav.home, path: this.project.rootPath() },
    { label: this.text().news.title },
  ]);

  constructor() {
    effect(() => {
      const copy = this.text().news;
      this.seo.apply({ title: copy.title, description: copy.lead, canonicalPath: this.project.path('news') });
    });
  }

  protected selectCategory(value: string | null): void {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { category: value && value !== ALL ? value : null },
      queryParamsHandling: 'merge',
      replaceUrl: true,
    });
  }
}
