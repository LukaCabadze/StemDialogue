import { inject } from '@angular/core';
import { CanActivateFn, CanDeactivateFn, Router } from '@angular/router';
import { map } from 'rxjs';

import { AdminAuth } from './admin-auth.service';
import { AdminProject } from './admin-project.service';
import { HasUnsavedChanges } from './editor-page';

/** Restores the session from the refresh cookie or sends the visitor to sign in. */
export const adminSessionGuard: CanActivateFn = (_route, state) => {
  const router = inject(Router);
  const project = inject(AdminProject);
  return inject(AdminAuth)
    .ensureSession()
    .pipe(map((ok) => (ok ? true : router.createUrlTree([project.url('login')], { queryParams: { returnUrl: state.url } }))));
};

/** Administrator-only screens (project settings, users, audit). */
export const adminRoleGuard: CanActivateFn = () => {
  const auth = inject(AdminAuth);
  return auth.isAdmin() ? true : inject(Router).createUrlTree([inject(AdminProject).url()]);
};

export const unsavedChangesGuard: CanDeactivateFn<HasUnsavedChanges> = (component) =>
  !component?.dirty() || confirm('შეუნახავი ცვლილებები დაიკარგება. გავიდეთ გვერდიდან?');
