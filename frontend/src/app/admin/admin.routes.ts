import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { Routes } from '@angular/router';

import { adminAuthInterceptor } from './core/admin-auth.interceptor';
import { AdminApi } from './core/admin-api.service';
import { AdminAuth } from './core/admin-auth.service';
import { adminRoleGuard, adminSessionGuard, unsavedChangesGuard } from './core/admin-guards';
import { AdminLookups } from './core/admin-lookups.service';
import { AdminProject, adminProjectGuard } from './core/admin-project.service';
import { Toasts } from './core/toasts.service';
import { AdminShell } from './layout/admin-shell';

/**
 * CMS at `/projects/{projectSlug}/admin`. Lazy-loaded, so none of it ships with the public site. It gets its own
 * HttpClient (bearer token + refresh on 401) instead of the public one (which adds `?lang=`).
 */
export const adminRoutes: Routes = [
  {
    path: '',
    providers: [
      provideHttpClient(withFetch(), withInterceptors([adminAuthInterceptor])),
      AdminAuth,
      AdminProject,
      AdminApi,
      AdminLookups,
      Toasts,
    ],
    canActivate: [adminProjectGuard],
    children: [
      {
        path: 'login',
        title: 'შესვლა — STEM Dialogue CMS',
        loadComponent: () => import('./pages/login-page').then((m) => m.LoginPage),
      },
      {
        path: '',
        component: AdminShell,
        canActivate: [adminSessionGuard],
        title: 'STEM Dialogue CMS',
        children: [
          { path: '', loadComponent: () => import('./pages/dashboard-page').then((m) => m.DashboardPage) },
          { path: 'news', loadComponent: () => import('./pages/content-lists').then((m) => m.NewsListPage) },
          {
            path: 'news/:id',
            canDeactivate: [unsavedChangesGuard],
            loadComponent: () => import('./pages/news-editor').then((m) => m.NewsEditorPage),
          },
          { path: 'events', loadComponent: () => import('./pages/content-lists').then((m) => m.EventsListPage) },
          {
            path: 'events/:id',
            canDeactivate: [unsavedChangesGuard],
            loadComponent: () => import('./pages/event-editor').then((m) => m.EventEditorPage),
          },
          { path: 'people', loadComponent: () => import('./pages/content-lists').then((m) => m.PeopleListPage) },
          {
            path: 'people/:id',
            canDeactivate: [unsavedChangesGuard],
            loadComponent: () => import('./pages/person-editor').then((m) => m.PersonEditorPage),
          },
          { path: 'resources', loadComponent: () => import('./pages/content-lists').then((m) => m.ResourcesListPage) },
          {
            path: 'resources/:id',
            canDeactivate: [unsavedChangesGuard],
            loadComponent: () => import('./pages/resource-editor').then((m) => m.ResourceEditorPage),
          },
          { path: 'activities', loadComponent: () => import('./pages/content-lists').then((m) => m.ActivitiesListPage) },
          {
            path: 'activities/:id',
            canDeactivate: [unsavedChangesGuard],
            loadComponent: () => import('./pages/activity-editor').then((m) => m.ActivityEditorPage),
          },
          { path: 'pages', loadComponent: () => import('./pages/pages-pages').then((m) => m.PagesListPage) },
          {
            path: 'pages/:id',
            canDeactivate: [unsavedChangesGuard],
            loadComponent: () => import('./pages/pages-pages').then((m) => m.PageEditorPage),
          },
          { path: 'milestones', loadComponent: () => import('./pages/milestones-page').then((m) => m.MilestonesPage) },
          { path: 'media', loadComponent: () => import('./pages/media-page').then((m) => m.MediaPage) },
          { path: 'taxonomy', loadComponent: () => import('./pages/taxonomy-page').then((m) => m.TaxonomyPage) },
          { path: 'organizations', loadComponent: () => import('./pages/organizations-page').then((m) => m.OrganizationsPage) },
          {
            path: 'organizations/:id',
            canDeactivate: [unsavedChangesGuard],
            loadComponent: () => import('./pages/organizations-page').then((m) => m.OrganizationEditorPage),
          },
          {
            path: 'project',
            data: { id: 'current' },
            canActivate: [adminRoleGuard],
            canDeactivate: [unsavedChangesGuard],
            loadComponent: () => import('./pages/project-page').then((m) => m.ProjectPage),
          },
          {
            path: 'users',
            canActivate: [adminRoleGuard],
            loadComponent: () => import('./pages/users-page').then((m) => m.UsersPage),
          },
          {
            path: 'audit',
            canActivate: [adminRoleGuard],
            loadComponent: () => import('./pages/audit-page').then((m) => m.AuditPage),
          },
          { path: 'account', loadComponent: () => import('./pages/account-page').then((m) => m.AccountPage) },
          { path: '**', redirectTo: '' },
        ],
      },
    ],
  },
];
