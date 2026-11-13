import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';

import { DateFormatService } from '../../../core/i18n/date-format.service';
import { LanguageService } from '../../../core/i18n/language.service';
import { EventDetailDto } from '../../../core/models';
import { Icon } from '../../../shared/ui/icon/icon';
import { IconName } from '../../../shared/ui/icon/icon-paths';

interface InfoCell {
  readonly icon: IconName;
  readonly label: string;
  readonly value: string;
}

/** 2×2 facts box under the event title: date, time, format/venue, field. */
@Component({
  selector: 'sd-event-info-grid',
  imports: [Icon],
  templateUrl: './event-info-grid.html',
  styleUrl: './event-info-grid.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EventInfoGrid {
  private readonly dates = inject(DateFormatService);
  private readonly text = inject(LanguageService).text;

  readonly event = input.required<EventDetailDto>();

  protected readonly cells = computed<readonly InfoCell[]>(() => {
    const event = this.event();
    const copy = this.text().events;
    const format = event.venue ? `${event.formatLabel} · ${event.venue}` : event.formatLabel;
    const fields = event.disciplines.map((term) => term.name).join(', ');

    return [
      { icon: 'calendar', label: copy.date, value: this.dates.format(event.startsAt, 'long') },
      { icon: 'clock', label: copy.time, value: this.dates.timeRange(event.startsAt, event.endsAt) },
      { icon: 'video', label: copy.format, value: format },
      { icon: 'atom', label: copy.discipline, value: fields || event.typeLabel },
    ];
  });
}
