import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { APP_ENVIRONMENT } from '../config/app-environment.token';

export type QueryValue = string | number | boolean | null | undefined;
export type QueryParams = Readonly<Record<string, QueryValue>>;

/**
 * Thin typed wrapper over HttpClient for the public API: builds URLs under the API base
 * and drops empty query values so callers can pass optional filters as-is.
 */
@Injectable({ providedIn: 'root' })
export class ApiClient {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(APP_ENVIRONMENT).apiBaseUrl.replace(/\/+$/, '');

  get<T>(path: string, query?: QueryParams): Observable<T> {
    return this.http.get<T>(this.url(path), { params: this.params(query) });
  }

  /** Resolves `projects/{slug}/...` segments, URL-encoding each dynamic part. */
  projectPath(projectSlug: string, ...segments: readonly string[]): string {
    return ['projects', projectSlug, ...segments].map((segment) => encodeURIComponent(segment)).join('/');
  }

  isApiUrl(url: string): boolean {
    return url.startsWith(this.baseUrl);
  }

  private url(path: string): string {
    return `${this.baseUrl}/${path.replace(/^\/+/, '')}`;
  }

  private params(query: QueryParams | undefined): HttpParams {
    let params = new HttpParams();
    if (!query) {
      return params;
    }
    for (const [key, value] of Object.entries(query)) {
      if (value === null || value === undefined || value === '') {
        continue;
      }
      params = params.set(key, String(value));
    }
    return params;
  }
}
