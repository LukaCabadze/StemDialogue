import { Injectable, computed, inject, signal } from '@angular/core';

import { APP_ENVIRONMENT } from '../config/app-environment.token';

/**
 * Holds the project slug of the site being viewed (`/projects/:projectSlug/...`).
 * The public shell writes it from the route; everything else reads it.
 */
@Injectable({ providedIn: 'root' })
export class ProjectContext {
  private readonly environment = inject(APP_ENVIRONMENT);
  private readonly slugState = signal(this.environment.defaultProjectSlug);

  readonly slug = this.slugState.asReadonly();
  readonly rootPath = computed(() => `/projects/${this.slug()}`);

  setSlug(slug: string): void {
    if (slug && slug !== this.slugState()) {
      this.slugState.set(slug);
    }
  }

  /** Absolute router path inside the current project, e.g. `path('news')` → `/projects/x/news`. */
  path(...segments: readonly (string | number)[]): string {
    const tail = segments
      .map((segment) => String(segment).replace(/^\/+|\/+$/g, ''))
      .filter((segment) => segment.length > 0)
      .join('/');
    return tail ? `${this.rootPath()}/${tail}` : this.rootPath();
  }
}
