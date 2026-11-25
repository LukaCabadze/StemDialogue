import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';

import { LanguageService } from '../../../core/i18n/language.service';
import { TeamMemberDto, TeamSectionDto } from '../../../core/models';
import { PersonDeck } from '../../../shared/ui/person-deck/person-deck';
import { DeckPerson } from '../../../shared/ui/person-deck/person-deck.model';
import { StaffTable } from '../staff-table/staff-table';

/**
 * One team-page section: heading (with the grant badge for official staff) and the members as a
 * clickable card deck; organisational staff who join in stages are listed as a table.
 */
@Component({
  selector: 'sd-team-section',
  imports: [PersonDeck, StaffTable],
  templateUrl: './team-section.html',
  styleUrl: './team-section.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[attr.id]': 'anchor()',
  },
})
export class TeamSection {
  protected readonly text = inject(LanguageService).text;

  readonly section = input.required<TeamSectionDto>();
  readonly index = input.required<string>();

  protected readonly anchor = computed(() => teamSectionAnchor(this.section()));
  protected readonly copy = computed(() => this.text().people.sections[this.section().role]);
  protected readonly asTable = computed(() => this.section().role === 'OrganizationalStaff');

  protected readonly deck = computed<readonly DeckPerson[]>(() =>
    this.section().current.map((member) => toDeckPerson(member, this.section().title)),
  );
}

export function teamSectionAnchor(section: TeamSectionDto): string {
  return `team-${section.role.toLowerCase()}`;
}

function toDeckPerson(member: TeamMemberDto, fallbackTitle: string): DeckPerson {
  const credentials = [member.academicTitle, member.organization].filter((part): part is string => !!part).join(', ');
  return {
    key: member.slug,
    name: member.fullName,
    photo: member.photo,
    title: member.roleTitle ?? fallbackTitle,
    meta: member.organization,
    text: member.shortBio ?? (credentials || null),
    path: member.path,
  };
}
