import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { CategoryCountDto, NewsCardDto, NewsDetailDto, NewsQuery, PagedResult } from '../models';
import { ApiClient } from './api-client.service';

@Injectable({ providedIn: 'root' })
export class NewsApi {
  private readonly api = inject(ApiClient);

  list(projectSlug: string, query: NewsQuery = {}): Observable<PagedResult<NewsCardDto>> {
    return this.api.get<PagedResult<NewsCardDto>>(this.api.projectPath(projectSlug, 'news'), { ...query });
  }

  categories(projectSlug: string): Observable<readonly CategoryCountDto[]> {
    return this.api.get<readonly CategoryCountDto[]>(this.api.projectPath(projectSlug, 'news', 'categories'));
  }

  get(projectSlug: string, slug: string): Observable<NewsDetailDto> {
    return this.api.get<NewsDetailDto>(this.api.projectPath(projectSlug, 'news', slug));
  }
}
