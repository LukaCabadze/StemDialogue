import { inject } from '@angular/core';
import { Routes } from '@angular/router';

import { APP_ENVIRONMENT } from './core/config/app-environment.token';
import { projectSlugGuard } from './core/project/project-slug.guard';
import { PublicShell } from './layout/public-shell/public-shell';

/** Public site: every page lives under `/projects/:projectSlug` (paths match the API's `path` fields). */
export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: () => `/projects/${inject(APP_ENVIRONMENT).defaultProjectSlug}`,
  },
  {
    path: 'projects/:projectSlug',
    component: PublicShell,
    canActivate: [projectSlugGuard],
    children: [
      {
        path: '',
        pathMatch: 'full',
        loadComponent: () => import('./features/home/home-page/home-page').then((m) => m.HomePage),
      },
      {
        path: 'about',
        loadComponent: () => import('./features/about/about-page/about-page').then((m) => m.AboutPage),
      },
      {
        path: 'news',
        loadComponent: () => import('./features/news/news-page/news-page').then((m) => m.NewsPage),
      },
      {
        path: 'news/:slug',
        loadComponent: () =>
          import('./features/news/news-detail-page/news-detail-page').then((m) => m.NewsDetailPage),
      },
      {
        path: 'stem-saturdays',
        loadComponent: () => import('./features/events/events-page/events-page').then((m) => m.EventsPage),
      },
      {
        path: 'stem-saturdays/:slug',
        loadComponent: () =>
          import('./features/events/event-detail-page/event-detail-page').then((m) => m.EventDetailPage),
      },
      {
        path: 'team',
        loadComponent: () => import('./features/people/team-page/team-page').then((m) => m.TeamPage),
      },
      {
        path: 'researchers',
        data: { directory: 'researchers' },
        loadComponent: () => import('./features/people/directory-page/directory-page').then((m) => m.DirectoryPage),
      },
      {
        path: 'mentors',
        data: { directory: 'mentors' },
        loadComponent: () => import('./features/people/directory-page/directory-page').then((m) => m.DirectoryPage),
      },
      ...['team', 'researchers', 'mentors'].map((section) => ({
        path: `${section}/:slug`,
        data: { section },
        loadComponent: () =>
          import('./features/people/person-page/person-page').then((m) => m.PersonPage),
      })),
      {
        path: 'stem-ambassadors',
        loadComponent: () =>
          import('./features/ambassadors/ambassadors-page/ambassadors-page').then((m) => m.AmbassadorsPage),
      },
      {
        path: 'stem-ambassadors/:slug',
        loadComponent: () =>
          import('./features/ambassadors/activity-detail-page/activity-detail-page').then((m) => m.ActivityDetailPage),
      },
      {
        path: 'resources',
        loadComponent: () => import('./features/resources/resources-page/resources-page').then((m) => m.ResourcesPage),
      },
      {
        path: 'resources/:slug',
        loadComponent: () =>
          import('./features/resources/resource-detail-page/resource-detail-page').then((m) => m.ResourceDetailPage),
      },
      {
        path: 'contact',
        loadComponent: () => import('./features/contact/contact-page/contact-page').then((m) => m.ContactPage),
      },
      {
        path: '**',
        loadComponent: () => import('./features/not-found/not-found-page/not-found-page').then((m) => m.NotFoundPage),
      },
    ],
  },
  {
    // Unknown top-level URLs still get the site chrome of the default project.
    path: '**',
    component: PublicShell,
    children: [
      {
        path: '**',
        loadComponent: () => import('./features/not-found/not-found-page/not-found-page').then((m) => m.NotFoundPage),
      },
    ],
  },
];
