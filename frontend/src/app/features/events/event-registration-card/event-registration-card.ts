import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';

import { DateFormatService } from '../../../core/i18n/date-format.service';
import { LanguageService } from '../../../core/i18n/language.service';
import { EventDetailDto } from '../../../core/models';
import { googleCalendarUrl } from '../../../core/util/calendar-link';
import { ActionLink } from '../../../shared/ui/action-link/action-link';
import { Icon } from '../../../shared/ui/icon/icon';
import { ShareButtons } from '../../../shared/ui/share-buttons/share-buttons';
import { LifecycleStepper } from '../lifecycle-stepper/lifecycle-stepper';

/** Sidebar card on the event page: lifecycle, registration / stream / recording actions, sharing. */
@Component({
  selector: 'sd-event-registration-card',
  imports: [ActionLink, Icon, LifecycleStepper, ShareButtons],
  templateUrl: './event-registration-card.html',
  styleUrl: './event-registration-card.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EventRegistrationCard {
  private readonly dates = inject(DateFormatService);
  protected readonly text = inject(LanguageService).text;

  readonly event = input.required<EventDetailDto>();

  protected readonly isUpcoming = computed(() => {
    const status = this.event().status;
    return status === 'Planned' || status === 'Postponed';
  });
  protected readonly hasArchive = computed(() => {
    const event = this.event();
    return !!event.recordingUrl || event.resources.length > 0 || event.gallery.length > 0;
  });
  protected readonly canRegister = computed(() => this.event().isRegistrationOpen && !!this.event().registrationUrl);

  /** "რეგისტრაცია ღიაა 20 ნოემბრამდე" when a deadline is set. */
  protected readonly deadlineText = computed(() => {
    const event = this.event();
    const copy = this.text().events;
    if (!event.registrationDeadline) {
      return copy.registrationOpen;
    }
    return `${copy.registrationOpenUntil} ${this.dates.format(event.registrationDeadline, 'until')}`;
  });

  protected readonly calendarUrl = computed(() => {
    const event = this.event();
    return googleCalendarUrl({
      title: event.title,
      startsAt: event.startsAt,
      endsAt: event.endsAt,
      details: event.streamUrl ?? event.summary,
      location: event.venue,
    });
  });
}
