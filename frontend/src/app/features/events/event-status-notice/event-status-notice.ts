import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';

import { DateFormatService } from '../../../core/i18n/date-format.service';
import { LanguageService } from '../../../core/i18n/language.service';
import { EventDetailDto } from '../../../core/models';
import { Icon } from '../../../shared/ui/icon/icon';

/** Red notice for postponed / cancelled meetings; the old link keeps working and shows the new date. */
@Component({
  selector: 'sd-event-status-notice',
  imports: [Icon],
  templateUrl: './event-status-notice.html',
  styleUrl: './event-status-notice.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { role: 'status' },
})
export class EventStatusNotice {
  private readonly dates = inject(DateFormatService);
  protected readonly text = inject(LanguageService).text;

  readonly event = input.required<EventDetailDto>();

  protected readonly title = computed(() => {
    const copy = this.text().events;
    return this.event().status === 'Cancelled' ? copy.cancelledTitle : copy.postponedTitle;
  });

  protected readonly dateLine = computed(() => {
    const event = this.event();
    if (event.status !== 'Postponed') {
      return null;
    }
    const copy = this.text().events;
    const newDate = `${copy.newDate}: ${this.dates.format(event.startsAt, 'long')}, ${this.dates.format(event.startsAt, 'time')}`;
    return event.originalStartsAt
      ? `${newDate} · ${copy.originalDate}: ${this.dates.format(event.originalStartsAt, 'long')}`
      : newDate;
  });
}
