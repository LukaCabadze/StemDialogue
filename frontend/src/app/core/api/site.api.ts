import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { SiteDto } from '../models';
import { ApiClient } from './api-client.service';

/** Layout data for one project: header, funding strip, navigation and footer. */
@Injectable({ providedIn: 'root' })
export class SiteApi {
  private readonly api = inject(ApiClient);

  getSite(projectSlug: string): Observable<SiteDto> {
    return this.api.get<SiteDto>(this.api.projectPath(projectSlug));
  }
}
