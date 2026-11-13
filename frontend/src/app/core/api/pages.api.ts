import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { AboutDto, ContactPageDto, MilestoneDto, PageDto } from '../models';
import { ApiClient } from './api-client.service';

/** CMS-managed static pages and the project milestones. */
@Injectable({ providedIn: 'root' })
export class PagesApi {
  private readonly api = inject(ApiClient);

  getAbout(projectSlug: string): Observable<AboutDto> {
    return this.api.get<AboutDto>(this.api.projectPath(projectSlug, 'about'));
  }

  getContact(projectSlug: string): Observable<ContactPageDto> {
    return this.api.get<ContactPageDto>(this.api.projectPath(projectSlug, 'contact'));
  }

  getPage(projectSlug: string, key: string): Observable<PageDto> {
    return this.api.get<PageDto>(this.api.projectPath(projectSlug, 'pages', key));
  }

  getMilestones(projectSlug: string): Observable<readonly MilestoneDto[]> {
    return this.api.get<readonly MilestoneDto[]>(this.api.projectPath(projectSlug, 'milestones'));
  }
}
