import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';

import { LanguageService } from '../../../core/i18n/language.service';
import { TeamMemberDto, TeamSectionDto } from '../../../core/models';
import { PersonCard } from '../../../shared/ui/person-card/person-card';
import { StaffTable } from '../staff-table/staff-table';

/**
 * One team-page section. Official grant staff sit in a highlighted panel with the grant badge;
 * organisational staff are listed as a table; everyone else as portrait cards.
 */
@Component({
  selector: 'sd-team-section',
  imports: [PersonCard, StaffTable],
  templateUrl: './team-section.html',
  styleUrl: './team-section.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.is-official]': 'section().isOfficialGrantStaff',
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

  protected description(member: TeamMemberDto): string | null {
    const parts = [member.academicTitle, member.organization].filter((part): part is string => !!part);
    return parts.length > 0 ? parts.join(', ') : member.shortBio;
  }
}

export function teamSectionAnchor(section: TeamSectionDto): string {
  return `team-${section.role.toLowerCase()}`;
}
