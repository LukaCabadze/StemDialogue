import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { AmbassadorsDto, PageQuery, PagedResult, SchoolActivityCardDto, SchoolActivityDetailDto } from '../models';
import { ApiClient } from './api-client.service';

@Injectable({ providedIn: 'root' })
export class AmbassadorsApi {
  private readonly api = inject(ApiClient);

  overview(projectSlug: string): Observable<AmbassadorsDto> {
    return this.api.get<AmbassadorsDto>(this.api.projectPath(projectSlug, 'stem-ambassadors'));
  }

  activities(projectSlug: string, query: PageQuery = {}): Observable<PagedResult<SchoolActivityCardDto>> {
    return this.api.get<PagedResult<SchoolActivityCardDto>>(
      this.api.projectPath(projectSlug, 'stem-ambassadors', 'activities'),
      { ...query },
    );
  }

  activity(projectSlug: string, slug: string): Observable<SchoolActivityDetailDto> {
    return this.api.get<SchoolActivityDetailDto>(this.api.projectPath(projectSlug, 'stem-ambassadors', 'activities', slug));
  }
}
