import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { PagedResult, PeopleQuery, PersonCardDto, PersonDetailDto, TeamDto } from '../models';
import { ApiClient } from './api-client.service';

export type PeopleDirectory = 'researchers' | 'mentors';

@Injectable({ providedIn: 'root' })
export class PeopleApi {
  private readonly api = inject(ApiClient);

  team(projectSlug: string): Observable<TeamDto> {
    return this.api.get<TeamDto>(this.api.projectPath(projectSlug, 'team'));
  }

  directory(projectSlug: string, directory: PeopleDirectory, query: PeopleQuery = {}): Observable<PagedResult<PersonCardDto>> {
    return this.api.get<PagedResult<PersonCardDto>>(this.api.projectPath(projectSlug, directory), { ...query });
  }

  person(projectSlug: string, slug: string): Observable<PersonDetailDto> {
    return this.api.get<PersonDetailDto>(this.api.projectPath(projectSlug, 'people', slug));
  }
}
