import { ChangeDetectionStrategy, Component, computed, effect, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';

import { EventsApi } from '../../../core/api/events.api';
import { LanguageService } from '../../../core/i18n/language.service';
import { EventFormat, EventQuery, EventScope } from '../../../core/models';
import { pagedResource } from '../../../core/paging/paged-resource';
import { ProjectContext } from '../../../core/project/project-context.service';
import { SeoService } from '../../../core/seo/seo.service';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { ErrorState } from '../../../shared/ui/error-state/error-state';
import { EventCard } from '../../../shared/ui/event-card/event-card';
import { LoadMore } from '../../../shared/ui/load-more/load-more';
import { LoadingState } from '../../../shared/ui/loading-state/loading-state';
import { PageHero } from '../../../shared/ui/page-hero/page-hero';
import { EventFilterChange, EventFilterValues, EventFilters } from '../event-filters/event-filters';

const SCOPES: readonly EventScope[] = ['Upcoming', 'Past', 'All'];
const FORMATS: readonly EventFormat[] = ['Online', 'InPerson', 'Hybrid'];
const PAGE_SIZE = 12;

/** STEM Saturdays listing with URL-synchronised filters (`?scope=Past&year=2026&discipline=…`). */
@Component({
  selector: 'sd-events-page',
  imports: [EmptyState, ErrorState, EventCard, EventFilters, LoadMore, LoadingState, PageHero],
  templateUrl: './events-page.html',
  styleUrl: './events-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EventsPage {
  private readonly api = inject(EventsApi);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly seo = inject(SeoService);
  private readonly languages = inject(LanguageService);
  protected readonly project = inject(ProjectContext);
  protected readonly text = this.languages.text;

  // Query parameters (bound by withComponentInputBinding).
  readonly scope = input<string>();
  readonly year = input<string>();
  readonly discipline = input<string>();
  readonly format = input<string>();
  readonly q = input<string>();

  protected readonly values = computed<EventFilterValues>(() => ({
    scope: SCOPES.find((scope) => scope === this.scope()) ?? 'Upcoming',
    year: this.year() || null,
    discipline: this.discipline() || null,
    format: this.format() || null,
    q: this.q() || null,
  }));

  private readonly query = computed(() => {
    const values = this.values();
    const year = Number(values.year);
    const query: EventQuery = {
      scope: values.scope,
      year: Number.isInteger(year) && year > 0 ? year : undefined,
      discipline: values.discipline ?? undefined,
      format: FORMATS.find((format) => format === values.format),
      q: values.q ?? undefined,
      pageSize: PAGE_SIZE,
    };
    return { slug: this.project.slug(), language: this.languages.language(), query };
  });

  protected readonly events = pagedResource({
    query: () => this.query(),
    load: (params, page) => this.api.list(params.slug, { ...params.query, page }),
  });

  protected readonly facets = rxResource({
    params: () => ({ slug: this.project.slug(), language: this.languages.language() }),
    stream: ({ params }) => this.api.filters(params.slug),
  });

  protected readonly crumbs = computed(() => [
    { label: this.text().nav.home, path: this.project.rootPath() },
    { label: this.text().events.title },
  ]);

  constructor() {
    effect(() => {
      const copy = this.text().events;
      this.seo.apply({ title: copy.title, description: copy.lead, canonicalPath: this.project.path('stem-saturdays') });
    });
  }

  protected applyFilters(change: EventFilterChange): void {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: change,
      queryParamsHandling: 'merge',
      replaceUrl: true,
    });
  }

  protected resetFilters(): void {
    void this.router.navigate([], { relativeTo: this.route, queryParams: { scope: this.values().scope } });
  }
}
