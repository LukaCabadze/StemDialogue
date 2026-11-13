import { ChangeDetectionStrategy, Component, inject, input, signal } from '@angular/core';

import { LanguageService } from '../../../core/i18n/language.service';
import { TeamMemberDto } from '../../../core/models';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { StaffTable } from '../staff-table/staff-table';

/** "Former contributors" archive: a dashed panel that expands into the table of former members. */
@Component({
  selector: 'sd-former-members',
  imports: [EmptyState, StaffTable],
  templateUrl: './former-members.html',
  styleUrl: './former-members.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FormerMembers {
  protected readonly text = inject(LanguageService).text;

  readonly members = input.required<readonly TeamMemberDto[]>();

  protected readonly expanded = signal(false);

  protected toggle(): void {
    this.expanded.update((open) => !open);
  }
}
