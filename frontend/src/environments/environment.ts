import { AppEnvironment } from './environment.model';

/** Production build: the SPA and the API are served from the same origin behind the reverse proxy. */
export const environment: AppEnvironment = {
  production: true,
  apiBaseUrl: '/api/v1',
  defaultProjectSlug: 'school2university',
  siteUrl: 'https://stemdialogue.com',
};
