import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { DateFormatService } from '../../../core/i18n/date-format.service';
import { LanguageService } from '../../../core/i18n/language.service';
import { TeamMemberDto } from '../../../core/models';

/** Name / role / period table for staff who join in stages (and for the former-members archive). */
@Component({
  selector: 'sd-staff-table',
  imports: [RouterLink],
  templateUrl: './staff-table.html',
  styleUrl: './staff-table.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StaffTable {
  private readonly dates = inject(DateFormatService);
  protected readonly text = inject(LanguageService).text;

  readonly members = input.required<readonly TeamMemberDto[]>();
  readonly caption = input.required<string>();

  protected period(member: TeamMemberDto): string {
    return this.dates.period(member.since, member.until, this.text().people.present);
  }
}
