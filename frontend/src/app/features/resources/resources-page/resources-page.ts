import { ChangeDetectionStrategy, Component, computed, effect, inject, input } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { ResourcesApi } from '../../../core/api/resources.api';
import { LanguageService } from '../../../core/i18n/language.service';
import { ResourceType } from '../../../core/models';
import { pagedResource } from '../../../core/paging/paged-resource';
import { ProjectContext } from '../../../core/project/project-context.service';
import { SeoService } from '../../../core/seo/seo.service';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { ErrorState } from '../../../shared/ui/error-state/error-state';
import { ChipOption, FilterChips } from '../../../shared/ui/filter-chips/filter-chips';
import { LoadMore } from '../../../shared/ui/load-more/load-more';
import { LoadingState } from '../../../shared/ui/loading-state/loading-state';
import { PageHero } from '../../../shared/ui/page-hero/page-hero';
import { SearchBox } from '../../../shared/ui/search-box/search-box';
import { ResourceCard } from '../resource-card/resource-card';

const ALL = 'all';
const PAGE_SIZE = 12;
const TYPES: readonly ResourceType[] = ['Presentation', 'Video', 'Document', 'Photo', 'Link'];

/** Resource library with type tabs and search (`?type=Video&q=…`). */
@Component({
  selector: 'sd-resources-page',
  imports: [EmptyState, ErrorState, FilterChips, LoadMore, LoadingState, PageHero, ResourceCard, SearchBox],
  templateUrl: './resources-page.html',
  styleUrl: './resources-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourcesPage {
  private readonly api = inject(ResourcesApi);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly seo = inject(SeoService);
  private readonly languages = inject(LanguageService);
  private readonly project = inject(ProjectContext);
  protected readonly text = this.languages.text;

  // Query parameters.
  readonly type = input<string>();
  readonly q = input<string>();

  protected readonly selectedType = computed(() => TYPES.find((type) => type === this.type()) ?? null);

  protected readonly typeOptions = computed<readonly ChipOption[]>(() => {
    const copy = this.text().resources;
    return [{ value: ALL, label: copy.allTypes }, ...TYPES.map((type) => ({ value: type, label: copy.types[type] }))];
  });

  protected readonly resources = pagedResource({
    query: () => ({
      slug: this.project.slug(),
      language: this.languages.language(),
      type: this.selectedType() ?? undefined,
      q: this.q() || undefined,
    }),
    load: (query, page) => this.api.list(query.slug, { type: query.type, q: query.q, page, pageSize: PAGE_SIZE }),
  });

  protected readonly crumbs = computed(() => [
    { label: this.text().nav.home, path: this.project.rootPath() },
    { label: this.text().resources.title },
  ]);

  constructor() {
    effect(() => {
      const copy = this.text().resources;
      this.seo.apply({ title: copy.title, description: copy.lead, canonicalPath: this.project.path('resources') });
    });
  }

  protected selectType(value: string | null): void {
    this.updateQuery({ type: value && value !== ALL ? value : null });
  }

  protected search(q: string): void {
    this.updateQuery({ q: q || null });
  }

  private updateQuery(queryParams: Record<string, string | null>): void {
    void this.router.navigate([], { relativeTo: this.route, queryParams, queryParamsHandling: 'merge', replaceUrl: true });
  }
}
