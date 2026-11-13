import { AppEnvironment } from './environment.model';

/** `ng serve`: requests to /api and /media are proxied to the ASP.NET API (see proxy.conf.json). */
export const environment: AppEnvironment = {
  production: false,
  apiBaseUrl: '/api/v1',
  defaultProjectSlug: 'school2university',
  siteUrl: 'http://localhost:4200',
};
