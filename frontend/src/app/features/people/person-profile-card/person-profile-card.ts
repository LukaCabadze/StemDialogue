import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';

import { DateFormatService } from '../../../core/i18n/date-format.service';
import { LanguageService } from '../../../core/i18n/language.service';
import { PersonDetailDto, PersonRole } from '../../../core/models';
import { Avatar } from '../../../shared/ui/avatar/avatar';
import { Icon } from '../../../shared/ui/icon/icon';
import { Pill, PillTone } from '../../../shared/ui/pill/pill';

/** Profile sidebar: portrait, role pills, name, title, organisation, period and external links. */
@Component({
  selector: 'sd-person-profile-card',
  imports: [Avatar, Icon, Pill],
  templateUrl: './person-profile-card.html',
  styleUrl: './person-profile-card.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PersonProfileCard {
  private readonly dates = inject(DateFormatService);
  protected readonly text = inject(LanguageService).text;

  readonly person = input.required<PersonDetailDto>();

  protected readonly title = computed(() =>
    [this.person().academicTitle, this.person().position].filter((part) => !!part).join(', '),
  );

  /** Earliest start to latest end across current roles ("ოქტომბერი 2026 – დღემდე"). */
  protected readonly period = computed(() => {
    const roles = this.person().roles;
    if (roles.length === 0) {
      return '';
    }
    const since = roles
      .map((role) => role.since)
      .filter((date): date is string => !!date)
      .sort()[0];
    const stillActive = roles.some((role) => role.isCurrent);
    const until = stillActive
      ? null
      : (roles
          .map((role) => role.until)
          .filter((date): date is string => !!date)
          .sort()
          .at(-1) ?? null);
    return this.dates.period(since, until, this.text().people.present);
  });

  protected toneOf(role: PersonRole): PillTone {
    if (role === 'Mentor') {
      return 'pink';
    }
    return 'blue';
  }
}
