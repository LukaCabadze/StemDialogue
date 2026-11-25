import { TestBed } from '@angular/core/testing';
import { Router, provideRouter, withComponentInputBinding, withRouterConfig } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';

import { routes } from '../app.routes';
import { APP_ENVIRONMENT } from '../core/config/app-environment.token';
import { LoginPage } from './pages/login-page';

describe('admin routes', () => {
  beforeEach(() => {
    // No API in unit tests: the session refresh fails, so protected pages lead to sign-in.
    vi.spyOn(globalThis, 'fetch').mockRejectedValue(new TypeError('offline'));
    TestBed.configureTestingModule({
      providers: [
        provideRouter(routes, withComponentInputBinding(), withRouterConfig({ paramsInheritanceStrategy: 'always' })),
        { provide: APP_ENVIRONMENT, useValue: { production: false, apiBaseUrl: '/api/v1', defaultProjectSlug: 'school2university', siteUrl: '' } },
      ],
    });
  });

  it('serves the admin under the project address', async () => {
    const harness = await RouterTestingHarness.create();
    const page = await harness.navigateByUrl('/projects/school2university/admin/login');
    expect(page).toBeInstanceOf(LoginPage);
    expect(TestBed.inject(Router).url).toBe('/projects/school2university/admin/login');
  });

  it('sends signed-out visitors to the project sign-in page', async () => {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/projects/school2university/admin/news');
    expect(TestBed.inject(Router).url).toBe(
      '/projects/school2university/admin/login?returnUrl=%2Fprojects%2Fschool2university%2Fadmin%2Fnews',
    );
  });

  it('redirects the /admin shortcut to the default project', async () => {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/admin/login?returnUrl=x');
    expect(TestBed.inject(Router).url).toBe('/projects/school2university/admin/login?returnUrl=x');
  });

  it('takes the project from the URL (sign-in links back to that project)', async () => {
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/projects/other-project/admin/login');
    const back = harness.routeNativeElement?.querySelector('.adm-login__back') as HTMLAnchorElement;
    expect(back.getAttribute('href')).toBe('/projects/other-project');
  });
});
