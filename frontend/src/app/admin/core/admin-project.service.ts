import { Injectable, Pipe, PipeTransform, inject, signal } from '@angular/core';
import { CanActivateFn } from '@angular/router';

import { APP_ENVIRONMENT } from '../../core/config/app-environment.token';

/**
 * The project the CMS is editing. It comes from the URL: the admin lives at
 * `/projects/{projectSlug}/admin/...`, next to that project's public pages.
 */
@Injectable()
export class AdminProject {
  readonly slug = signal<string>(inject(APP_ENVIRONMENT).defaultProjectSlug);

  /** Absolute admin URL, e.g. `url('news')` → `/projects/school2university/admin/news`. */
  url(path = ''): string {
    const tail = path.replace(/^\/+/, '');
    return `/projects/${encodeURIComponent(this.slug())}/admin${tail ? `/${tail}` : ''}`;
  }

  /** Public page of the project, e.g. `publicUrl()` → `/projects/school2university`. */
  publicUrl(): string {
    return `/projects/${encodeURIComponent(this.slug())}`;
  }
}

/** Copies `:projectSlug` from the URL into AdminProject before any admin page opens. */
export const adminProjectGuard: CanActivateFn = (route) => {
  const slug = route.paramMap.get('projectSlug');
  if (slug) {
    inject(AdminProject).slug.set(slug);
  }
  return true;
};

/** `'news' | admUrl` → `/projects/{slug}/admin/news` (for routerLink in templates). */
@Pipe({ name: 'admUrl', pure: false })
export class AdmUrlPipe implements PipeTransform {
  private readonly project = inject(AdminProject);

  transform(path: string): string {
    return this.project.url(path);
  }
}
