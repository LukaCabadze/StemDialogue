import { ChangeDetectionStrategy, Component, computed, inject, input, output } from '@angular/core';

import { LanguageService } from '../../../core/i18n/language.service';
import { EventFiltersDto, EventScope } from '../../../core/models';
import { ChipOption, FilterChips } from '../../../shared/ui/filter-chips/filter-chips';
import { SearchBox } from '../../../shared/ui/search-box/search-box';

export interface EventFilterValues {
  readonly scope: EventScope;
  readonly year: string | null;
  readonly discipline: string | null;
  readonly format: string | null;
  readonly q: string | null;
}

export type EventFilterChange = Partial<Record<keyof EventFilterValues, string | null>>;

/** Scope switch (upcoming / past / all), year, field and format selects, and text search. */
@Component({
  selector: 'sd-event-filters',
  imports: [FilterChips, SearchBox],
  templateUrl: './event-filters.html',
  styleUrl: './event-filters.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EventFilters {
  protected readonly text = inject(LanguageService).text;

  readonly facets = input<EventFiltersDto | null>(null);
  readonly values = input.required<EventFilterValues>();
  readonly changed = output<EventFilterChange>();

  protected readonly scopes = computed<readonly ChipOption<EventScope>[]>(() => {
    const copy = this.text().events;
    return [
      { value: 'Upcoming', label: copy.scopeUpcoming },
      { value: 'Past', label: copy.scopePast },
      { value: 'All', label: copy.scopeAll },
    ];
  });

  protected setScope(scope: EventScope | null): void {
    this.changed.emit({ scope });
  }

  protected setSelect(key: 'year' | 'discipline' | 'format', value: string): void {
    this.changed.emit({ [key]: value || null });
  }

  protected setSearch(q: string): void {
    this.changed.emit({ q: q || null });
  }
}
