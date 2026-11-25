import { HttpErrorResponse, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';

import { AdminAuth } from './admin-auth.service';
import { AdminProject } from './admin-project.service';

const withToken = (request: HttpRequest<unknown>, token: string | null) =>
  token ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : request;

/**
 * Adds the bearer token to CMS calls. On 401 it refreshes the session once and retries;
 * if that fails the editor is sent to the sign-in page.
 */
export const adminAuthInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AdminAuth);
  const router = inject(Router);
  const project = inject(AdminProject);
  const isRefreshOrLogin = auth.isAuthUrl(request.url) && /\/(login|refresh)$/.test(request.url);

  return next(withToken(request, auth.accessToken())).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse) || error.status !== 401 || isRefreshOrLogin) {
        return throwError(() => error);
      }
      return auth.refresh().pipe(
        switchMap((token) => {
          if (!token) {
            void router.navigate([project.url('login')], { queryParams: { returnUrl: router.url } });
            return throwError(() => error);
          }
          return next(withToken(request, token));
        }),
      );
    }),
  );
};
