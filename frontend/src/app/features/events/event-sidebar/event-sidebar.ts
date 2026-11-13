import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { LanguageService } from '../../../core/i18n/language.service';
import { EventDetailDto, TermDto } from '../../../core/models';
import { ProjectContext } from '../../../core/project/project-context.service';
import { toTbilisiParts } from '../../../core/i18n/tbilisi-date';
import { SdDatePipe } from '../../../shared/pipes/sd-date.pipe';
import { Pill } from '../../../shared/ui/pill/pill';

interface ArchiveFilterLink {
  readonly label: string;
  readonly query: Readonly<Record<string, string>>;
}

/** Event page sidebar: topics, related news and (after the meeting) quick archive filters. */
@Component({
  selector: 'sd-event-sidebar',
  imports: [RouterLink, Pill, SdDatePipe],
  templateUrl: './event-sidebar.html',
  styleUrl: './event-sidebar.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'sd-sidebar' },
})
export class EventSidebar {
  private readonly languages = inject(LanguageService);
  private readonly project = inject(ProjectContext);
  protected readonly text = this.languages.text;
  protected readonly language = this.languages.language;

  readonly event = input.required<EventDetailDto>();

  protected readonly listPath = computed(() => this.project.path('stem-saturdays'));
  protected readonly topics = computed<readonly TermDto[]>(() => [...this.event().tags, ...this.event().disciplines]);
  protected readonly isPast = computed(() => this.event().status === 'Completed');

  protected readonly archiveFilters = computed<readonly ArchiveFilterLink[]>(() => {
    const event = this.event();
    const parts = toTbilisiParts(event.startsAt);
    if (!parts) {
      return [];
    }
    const months = this.text().date.months;
    return [
      { label: String(parts.year), query: { scope: 'Past', year: String(parts.year) } },
      { label: months[parts.month] ?? '', query: { scope: 'Past', year: String(parts.year) } },
      ...event.disciplines.map((term) => ({ label: term.name, query: { scope: 'Past', discipline: term.slug } })),
    ];
  });
}
