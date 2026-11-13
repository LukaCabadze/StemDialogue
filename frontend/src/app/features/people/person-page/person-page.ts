import { ChangeDetectionStrategy, Component, computed, effect, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';

import { PeopleApi } from '../../../core/api/people.api';
import { LanguageService } from '../../../core/i18n/language.service';
import { EventCardDto, PersonDetailDto } from '../../../core/models';
import { ProjectContext } from '../../../core/project/project-context.service';
import { SeoService } from '../../../core/seo/seo.service';
import { Breadcrumbs, Crumb } from '../../../shared/ui/breadcrumbs/breadcrumbs';
import { ErrorState } from '../../../shared/ui/error-state/error-state';
import { EventRow } from '../../../shared/ui/event-row/event-row';
import { LoadingState } from '../../../shared/ui/loading-state/loading-state';
import { ResourceItem } from '../../../shared/ui/resource-item/resource-item';
import { RichText } from '../../../shared/ui/rich-text/rich-text';
import { PersonContribution } from '../person-contribution/person-contribution';
import { PersonProfileCard } from '../person-profile-card/person-profile-card';

type PeopleSection = 'team' | 'researchers' | 'mentors';

/** Public profile (one person = one profile, several roles): bio, contribution, meetings, materials. */
@Component({
  selector: 'sd-person-page',
  imports: [
    Breadcrumbs,
    ErrorState,
    EventRow,
    LoadingState,
    PersonContribution,
    PersonProfileCard,
    ResourceItem,
    RichText,
  ],
  templateUrl: './person-page.html',
  styleUrl: './person-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PersonPage {
  private readonly api = inject(PeopleApi);
  private readonly seo = inject(SeoService);
  private readonly languages = inject(LanguageService);
  private readonly project = inject(ProjectContext);
  protected readonly text = this.languages.text;

  /** Route parameter `:slug`. */
  readonly slug = input.required<string>();
  /** Route data: which listing the visitor came through. */
  readonly section = input<PeopleSection>('team');

  protected readonly person = rxResource({
    params: () => ({ project: this.project.slug(), slug: this.slug(), language: this.languages.language() }),
    stream: ({ params }) => this.api.person(params.project, params.slug),
  });

  protected readonly data = computed<PersonDetailDto | null>(() => (this.person.hasValue() ? this.person.value() : null));
  protected readonly events = computed<readonly EventCardDto[]>(() => {
    const person = this.data();
    return person ? [...person.upcomingEvents, ...person.pastEvents] : [];
  });

  protected readonly crumbs = computed<readonly Crumb[]>(() => {
    const nav = this.text().nav;
    const section = this.section();
    const sectionLabel = section === 'researchers' ? nav.researchers : section === 'mentors' ? nav.mentors : nav.team;
    return [
      { label: nav.home, path: this.project.rootPath() },
      { label: nav.people },
      { label: sectionLabel, path: this.project.path(section) },
      { label: this.data()?.fullName ?? '' },
    ];
  });

  constructor() {
    effect(() => {
      const person = this.data();
      if (person) {
        this.seo.apply(person.seo);
      }
    });
  }
}
