import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, catchError, finalize, map, of, shareReplay, tap } from 'rxjs';

import { APP_ENVIRONMENT } from '../../core/config/app-environment.token';
import { AccessTokenResponse, AuthUser } from './admin.models';

/**
 * CMS session. The access token lives only in memory; the refresh token is an HttpOnly cookie
 * (`sd_refresh`, path `/api/v1/auth`) that the API rotates on every refresh.
 */
@Injectable()
export class AdminAuth {
  private readonly http = inject(HttpClient);
  private readonly base = `${inject(APP_ENVIRONMENT).apiBaseUrl.replace(/\/+$/, '')}/auth`;

  private token: string | null = null;
  private refreshInFlight: Observable<string | null> | null = null;

  readonly user = signal<AuthUser | null>(null);
  readonly isAdmin = computed(() => this.user()?.roles.includes('Admin') ?? false);

  accessToken(): string | null {
    return this.token;
  }

  isAuthUrl(url: string): boolean {
    return url.startsWith(this.base);
  }

  login(email: string, password: string): Observable<AuthUser> {
    return this.http
      .post<AccessTokenResponse>(`${this.base}/login`, { email, password })
      .pipe(map((response) => this.accept(response)));
  }

  /** Exchanges the refresh cookie for a new access token. Concurrent callers share one request. */
  refresh(): Observable<string | null> {
    this.refreshInFlight ??= this.http.post<AccessTokenResponse>(`${this.base}/refresh`, null).pipe(
      map((response) => {
        this.accept(response);
        return response.accessToken;
      }),
      catchError(() => {
        this.clear();
        return of(null);
      }),
      finalize(() => (this.refreshInFlight = null)),
      shareReplay(1),
    );
    return this.refreshInFlight;
  }

  /** Restores a session after a page reload; true when the visitor is signed in. */
  ensureSession(): Observable<boolean> {
    if (this.user()) {
      return of(true);
    }
    return this.refresh().pipe(map((token) => token !== null));
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${this.base}/logout`, null).pipe(
      catchError(() => of(undefined)),
      tap(() => this.clear()),
    );
  }

  changePassword(currentPassword: string, newPassword: string): Observable<void> {
    return this.http
      .post<void>(`${this.base}/change-password`, { currentPassword, newPassword })
      .pipe(tap(() => this.clear()));
  }

  clear(): void {
    this.token = null;
    this.user.set(null);
  }

  private accept(response: AccessTokenResponse): AuthUser {
    this.token = response.accessToken;
    this.user.set(response.user);
    return response.user;
  }
}
