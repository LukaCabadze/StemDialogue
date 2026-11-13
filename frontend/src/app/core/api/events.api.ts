import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { EventCardDto, EventDetailDto, EventFiltersDto, EventQuery, PagedResult } from '../models';
import { ApiClient } from './api-client.service';

/** STEM Saturdays and other project events (one URL across the whole lifecycle). */
@Injectable({ providedIn: 'root' })
export class EventsApi {
  private readonly api = inject(ApiClient);

  list(projectSlug: string, query: EventQuery = {}): Observable<PagedResult<EventCardDto>> {
    return this.api.get<PagedResult<EventCardDto>>(this.api.projectPath(projectSlug, 'stem-saturdays'), { ...query });
  }

  filters(projectSlug: string): Observable<EventFiltersDto> {
    return this.api.get<EventFiltersDto>(this.api.projectPath(projectSlug, 'stem-saturdays', 'filters'));
  }

  get(projectSlug: string, slug: string): Observable<EventDetailDto> {
    return this.api.get<EventDetailDto>(this.api.projectPath(projectSlug, 'stem-saturdays', slug));
  }
}
