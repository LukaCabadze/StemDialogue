import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';

import { ProjectContext } from './project-context.service';

/** Copies `:projectSlug` into ProjectContext before any project page is created. */
export const projectSlugGuard: CanActivateFn = (route) => {
  const slug = route.paramMap.get('projectSlug');
  if (slug) {
    inject(ProjectContext).setSlug(slug);
  }
  return true;
};
