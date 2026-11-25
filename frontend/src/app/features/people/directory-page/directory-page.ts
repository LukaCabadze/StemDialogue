import { ChangeDetectionStrategy, Component, computed, effect, inject, input } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { PeopleApi, PeopleDirectory } from '../../../core/api/people.api';
import { LanguageService } from '../../../core/i18n/language.service';
import { PersonCardDto } from '../../../core/models';
import { pagedResource } from '../../../core/paging/paged-resource';
import { ProjectContext } from '../../../core/project/project-context.service';
import { SeoService } from '../../../core/seo/seo.service';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { ErrorState } from '../../../shared/ui/error-state/error-state';
import { LoadMore } from '../../../shared/ui/load-more/load-more';
import { LoadingState } from '../../../shared/ui/loading-state/loading-state';
import { PageHero } from '../../../shared/ui/page-hero/page-hero';
import { PersonDeck } from '../../../shared/ui/person-deck/person-deck';
import { DeckPerson } from '../../../shared/ui/person-deck/person-deck.model';
import { SearchBox } from '../../../shared/ui/search-box/search-box';

const PAGE_SIZE = 12;

/** Researchers or mentors directory (chosen by route data), searchable by name. */
@Component({
  selector: 'sd-directory-page',
  imports: [EmptyState, ErrorState, LoadMore, LoadingState, PageHero, PersonDeck, SearchBox],
  templateUrl: './directory-page.html',
  styleUrl: './directory-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DirectoryPage {
  private readonly api = inject(PeopleApi);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly seo = inject(SeoService);
  private readonly languages = inject(LanguageService);
  private readonly project = inject(ProjectContext);
  protected readonly text = this.languages.text;

  /** Route data. */
  readonly directory = input.required<PeopleDirectory>();
  /** Query parameter. */
  readonly q = input<string>();

  protected readonly title = computed(() =>
    this.directory() === 'mentors' ? this.text().people.mentorsTitle : this.text().people.researchersTitle,
  );
  protected readonly lead = computed(() =>
    this.directory() === 'mentors' ? this.text().people.mentorsLead : this.text().people.researchersLead,
  );
  protected readonly crumbs = computed(() => [
    { label: this.text().nav.home, path: this.project.rootPath() },
    { label: this.text().nav.people },
    { label: this.title() },
  ]);

  protected readonly people = pagedResource({
    query: () => ({
      slug: this.project.slug(),
      directory: this.directory(),
      language: this.languages.language(),
      q: this.q() || undefined,
    }),
    load: (query, page) => this.api.directory(query.slug, query.directory, { q: query.q, page, pageSize: PAGE_SIZE }),
  });

  constructor() {
    effect(() => {
      this.seo.apply({ title: this.title(), description: this.lead(), canonicalPath: this.project.path(this.directory()) });
    });
  }

  protected readonly deck = computed<readonly DeckPerson[]>(() =>
    this.people.items().map((person) => this.toDeckPerson(person)),
  );

  private toDeckPerson(person: PersonCardDto): DeckPerson {
    const credentials = [person.academicTitle, person.organization].filter((part): part is string => !!part).join(', ');
    return {
      key: person.slug,
      name: person.fullName,
      photo: person.photo,
      title: person.position ?? this.title(),
      meta: person.organization,
      text: person.shortBio ?? (credentials || null),
      path: person.path,
    };
  }

  protected search(q: string): void {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { q: q || null },
      queryParamsHandling: 'merge',
      replaceUrl: true,
    });
  }
}
