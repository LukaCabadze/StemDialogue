import { ChangeDetectionStrategy, Component, computed, effect, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';

import { PeopleApi } from '../../../core/api/people.api';
import { LanguageService } from '../../../core/i18n/language.service';
import { TeamDto, TeamMemberDto } from '../../../core/models';
import { ProjectContext } from '../../../core/project/project-context.service';
import { SeoService } from '../../../core/seo/seo.service';
import { ErrorState } from '../../../shared/ui/error-state/error-state';
import { LoadingState } from '../../../shared/ui/loading-state/loading-state';
import { PageHero } from '../../../shared/ui/page-hero/page-hero';
import { FormerMembers } from '../former-members/former-members';
import { TeamSection, teamSectionAnchor } from '../team-section/team-section';

/** "Our team": official grant staff, consultants, technology team, organisational staff, former members. */
@Component({
  selector: 'sd-team-page',
  imports: [RouterLink, ErrorState, FormerMembers, LoadingState, PageHero, TeamSection],
  templateUrl: './team-page.html',
  styleUrl: './team-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TeamPage {
  private readonly api = inject(PeopleApi);
  private readonly seo = inject(SeoService);
  private readonly languages = inject(LanguageService);
  private readonly project = inject(ProjectContext);
  protected readonly text = this.languages.text;
  protected readonly anchorOf = teamSectionAnchor;

  protected readonly team = rxResource({
    params: () => ({ slug: this.project.slug(), language: this.languages.language() }),
    stream: ({ params }) => this.api.team(params.slug),
  });

  protected readonly data = computed<TeamDto | null>(() => (this.team.hasValue() ? this.team.value() : null));
  protected readonly sections = computed(() => (this.data()?.sections ?? []).filter((section) => section.current.length > 0));
  protected readonly former = computed<readonly TeamMemberDto[]>(() =>
    (this.data()?.sections ?? []).flatMap((section) => section.former),
  );

  protected readonly title = computed(() => this.data()?.page?.title ?? this.text().people.teamTitle);
  protected readonly lead = computed(() => this.data()?.page?.lead ?? this.text().people.teamLead);
  protected readonly crumbs = computed(() => [
    { label: this.text().nav.home, path: this.project.rootPath() },
    { label: this.text().nav.people },
    { label: this.title() },
  ]);

  constructor() {
    effect(() => {
      const page = this.data()?.page;
      if (page) {
        this.seo.apply(page.seo);
      } else {
        this.seo.apply({ title: this.title(), description: this.lead(), canonicalPath: this.project.path('team') });
      }
    });
  }

  protected indexOf(position: number): string {
    return (position + 1).toString().padStart(2, '0');
  }
}
