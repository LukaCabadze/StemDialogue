import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { DateFormatService } from '../../../core/i18n/date-format.service';
import { LanguageService } from '../../../core/i18n/language.service';
import { EventCardDto } from '../../../core/models';
import { Avatar } from '../avatar/avatar';
import { DateChip } from '../date-chip/date-chip';
import { Icon } from '../icon/icon';
import { Pill } from '../pill/pill';
import { StatusPill } from '../status-pill/status-pill';

/** STEM Saturday card: date tile, status/format, title, time and fields, speaker, registration footer. */
@Component({
  selector: 'sd-event-card',
  imports: [RouterLink, Avatar, DateChip, Icon, Pill, StatusPill],
  templateUrl: './event-card.html',
  styleUrl: './event-card.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EventCard {
  private readonly dates = inject(DateFormatService);
  protected readonly text = inject(LanguageService).text;

  readonly event = input.required<EventCardDto>();
  /** Heading level inside the surrounding section. */
  readonly headingLevel = input<2 | 3>(3);

  protected readonly time = computed(() => {
    const event = this.event();
    return this.dates.timeRange(event.startsAt, event.endsAt);
  });
  protected readonly disciplines = computed(() =>
    this.event()
      .disciplines.map((term) => term.name)
      .join(', '),
  );
  protected readonly speaker = computed(() => this.event().speakers[0] ?? null);
  protected readonly canRegister = computed(() => this.event().isUpcoming && !!this.event().registrationUrl);
}
