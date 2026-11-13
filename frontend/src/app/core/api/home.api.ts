import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { HomeDto } from '../models';
import { ApiClient } from './api-client.service';

@Injectable({ providedIn: 'root' })
export class HomeApi {
  private readonly api = inject(ApiClient);

  getHome(projectSlug: string): Observable<HomeDto> {
    return this.api.get<HomeDto>(this.api.projectPath(projectSlug, 'home'));
  }
}
