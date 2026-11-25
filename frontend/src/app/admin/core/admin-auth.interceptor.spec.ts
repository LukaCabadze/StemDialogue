import { HttpClient, HttpErrorResponse, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { APP_ENVIRONMENT } from '../../core/config/app-environment.token';
import { adminAuthInterceptor } from './admin-auth.interceptor';
import { AdminAuth } from './admin-auth.service';
import { AdminProject } from './admin-project.service';
import { toProblem } from './problem';

const user = { id: 'u1', email: 'a@b.c', fullName: 'Editor', roles: ['Editor'] };

describe('adminAuthInterceptor', () => {
  let http: HttpClient;
  let backend: HttpTestingController;
  let auth: AdminAuth;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([adminAuthInterceptor])),
        provideHttpClientTesting(),
        AdminAuth,
        AdminProject,
        { provide: APP_ENVIRONMENT, useValue: { production: false, apiBaseUrl: '/api/v1', defaultProjectSlug: 'p', siteUrl: '' } },
      ],
    });
    http = TestBed.inject(HttpClient);
    backend = TestBed.inject(HttpTestingController);
    auth = TestBed.inject(AdminAuth);
  });

  afterEach(() => backend.verify());

  async function signIn(): Promise<void> {
    const login = firstValueFrom(auth.login('a@b.c', 'pw'));
    backend.expectOne('/api/v1/auth/login').flush({ accessToken: 'token-1', expiresAtUtc: '', user });
    await login;
  }

  it('sends the access token with CMS requests', async () => {
    await signIn();
    const call = firstValueFrom(http.get('/api/v1/admin/people'));
    const request = backend.expectOne('/api/v1/admin/people');
    expect(request.request.headers.get('Authorization')).toBe('Bearer token-1');
    request.flush({ items: [] });
    await call;
  });

  it('refreshes once on 401 and retries with the new token', async () => {
    await signIn();
    const call = firstValueFrom(http.get<{ ok: boolean }>('/api/v1/admin/news'));

    backend.expectOne('/api/v1/admin/news').flush(null, { status: 401, statusText: 'Unauthorized' });
    backend.expectOne('/api/v1/auth/refresh').flush({ accessToken: 'token-2', expiresAtUtc: '', user });
    const retry = backend.expectOne('/api/v1/admin/news');
    expect(retry.request.headers.get('Authorization')).toBe('Bearer token-2');
    retry.flush({ ok: true });

    expect(await call).toEqual({ ok: true });
  });

  it('signs out and goes to the login page when the refresh fails', async () => {
    await signIn();
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    const call = firstValueFrom(http.get('/api/v1/admin/news'));

    backend.expectOne('/api/v1/admin/news').flush(null, { status: 401, statusText: 'Unauthorized' });
    backend.expectOne('/api/v1/auth/refresh').flush(null, { status: 401, statusText: 'Unauthorized' });

    await expect(call).rejects.toBeInstanceOf(HttpErrorResponse);
    expect(auth.user()).toBeNull();
    expect(navigate).toHaveBeenCalledWith(['/projects/p/admin/login'], expect.anything());
  });
});

describe('toProblem', () => {
  it('maps validation problems to per-field messages', () => {
    const problem = toProblem(
      new HttpErrorResponse({
        status: 400,
        error: { detail: 'One or more fields are invalid.', errors: { title: ['Georgian text is required.'] } },
      }),
    );
    expect(problem.detail).toBeNull();
    expect(problem.fields).toEqual([{ field: 'title', messages: ['ქართული ტექსტი სავალდებულოა.'] }]);
  });

  it('explains concurrency conflicts', () => {
    const problem = toProblem(
      new HttpErrorResponse({ status: 409, error: { detail: 'This item was changed by someone else. Reload it and apply your edits again.' } }),
    );
    expect(problem.title).toContain('სხვამ შეცვალა');
  });
});
