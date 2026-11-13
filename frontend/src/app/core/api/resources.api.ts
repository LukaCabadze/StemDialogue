import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { PagedResult, ResourceCardDto, ResourceDetailDto, ResourceQuery } from '../models';
import { ApiClient } from './api-client.service';

@Injectable({ providedIn: 'root' })
export class ResourcesApi {
  private readonly api = inject(ApiClient);

  list(projectSlug: string, query: ResourceQuery = {}): Observable<PagedResult<ResourceCardDto>> {
    return this.api.get<PagedResult<ResourceCardDto>>(this.api.projectPath(projectSlug, 'resources'), { ...query });
  }

  get(projectSlug: string, slug: string): Observable<ResourceDetailDto> {
    return this.api.get<ResourceDetailDto>(this.api.projectPath(projectSlug, 'resources', slug));
  }
}
