import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';

import { LanguageService } from '../../../core/i18n/language.service';
import { EventCardDto } from '../../../core/models';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { EventCard } from '../../../shared/ui/event-card/event-card';
import { SectionHeader } from '../../../shared/ui/section-header/section-header';

/** "02 · Upcoming meetings": three event cards on the soft background. */
@Component({
  selector: 'sd-home-upcoming',
  imports: [EmptyState, EventCard, SectionHeader],
  templateUrl: './home-upcoming.html',
  styleUrl: './home-upcoming.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeUpcoming {
  protected readonly text = inject(LanguageService).text;

  readonly events = input.required<readonly EventCardDto[]>();
  readonly listPath = input.required<string>();
  readonly index = input('02');
}
