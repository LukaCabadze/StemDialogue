import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';

import { LanguageService } from '../../../core/i18n/language.service';
import { EventDetailDto } from '../../../core/models';
import { ProjectContext } from '../../../core/project/project-context.service';
import { Breadcrumbs } from '../../../shared/ui/breadcrumbs/breadcrumbs';
import { Pill } from '../../../shared/ui/pill/pill';
import { StatusPill } from '../../../shared/ui/status-pill/status-pill';
import { EventInfoGrid } from '../event-info-grid/event-info-grid';
import { EventRegistrationCard } from '../event-registration-card/event-registration-card';

/** Top band of the event page: breadcrumbs, badges, title, lead, facts grid and the registration card. */
@Component({
  selector: 'sd-event-detail-header',
  imports: [Breadcrumbs, EventInfoGrid, EventRegistrationCard, Pill, StatusPill],
  templateUrl: './event-detail-header.html',
  styleUrl: './event-detail-header.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EventDetailHeader {
  private readonly project = inject(ProjectContext);
  private readonly text = inject(LanguageService).text;

  readonly event = input.required<EventDetailDto>();

  protected readonly crumbs = computed(() => [
    { label: this.text().nav.home, path: this.project.rootPath() },
    { label: this.text().events.title, path: this.project.path('stem-saturdays') },
    { label: this.event().title },
  ]);
}
