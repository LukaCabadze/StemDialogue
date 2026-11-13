import { ChangeDetectionStrategy, Component, computed, effect, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';

import { NewsApi } from '../../../core/api/news.api';
import { LanguageService } from '../../../core/i18n/language.service';
import { NewsDetailDto } from '../../../core/models';
import { ProjectContext } from '../../../core/project/project-context.service';
import { SeoService } from '../../../core/seo/seo.service';
import { Breadcrumbs } from '../../../shared/ui/breadcrumbs/breadcrumbs';
import { ErrorState } from '../../../shared/ui/error-state/error-state';
import { LoadingState } from '../../../shared/ui/loading-state/loading-state';
import { NewsCard } from '../../../shared/ui/news-card/news-card';
import { SectionHeader } from '../../../shared/ui/section-header/section-header';
import { NewsArticle } from '../news-article/news-article';
import { NewsSidebar } from '../news-sidebar/news-sidebar';

/** Single news article with its sidebar and related articles. */
@Component({
  selector: 'sd-news-detail-page',
  imports: [Breadcrumbs, ErrorState, LoadingState, NewsArticle, NewsCard, NewsSidebar, SectionHeader],
  templateUrl: './news-detail-page.html',
  styleUrl: './news-detail-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NewsDetailPage {
  private readonly api = inject(NewsApi);
  private readonly seo = inject(SeoService);
  private readonly languages = inject(LanguageService);
  protected readonly project = inject(ProjectContext);
  protected readonly text = this.languages.text;

  /** Route parameter `:slug`. */
  readonly slug = input.required<string>();

  protected readonly article = rxResource({
    params: () => ({ project: this.project.slug(), slug: this.slug(), language: this.languages.language() }),
    stream: ({ params }) => this.api.get(params.project, params.slug),
  });

  private readonly categories = rxResource({
    params: () => ({ project: this.project.slug(), language: this.languages.language() }),
    stream: ({ params }) => this.api.categories(params.project),
  });

  protected readonly data = computed<NewsDetailDto | null>(() => (this.article.hasValue() ? this.article.value() : null));
  protected readonly categoryList = computed(() => (this.categories.hasValue() ? this.categories.value() : []));

  protected readonly crumbs = computed(() => [
    { label: this.text().nav.home, path: this.project.rootPath() },
    { label: this.text().news.title, path: this.project.path('news') },
    { label: this.data()?.title ?? '' },
  ]);

  constructor() {
    effect(() => {
      const article = this.data();
      if (article) {
        this.seo.apply(article.seo);
      }
    });
  }
}
