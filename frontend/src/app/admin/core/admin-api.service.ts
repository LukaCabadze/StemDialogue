import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { APP_ENVIRONMENT } from '../../core/config/app-environment.token';
import {
  ActivityAdmin,
  ActivityListItem,
  AuditEntry,
  Dashboard,
  EventAdmin,
  EventListItem,
  Lookups,
  MediaAdmin,
  MediaUsage,
  MilestoneAdmin,
  NewsAdmin,
  NewsListItem,
  OrganizationAdmin,
  PageAdmin,
  PageListItem,
  Paged,
  PersonAdmin,
  PersonListItem,
  ProjectAdmin,
  ProjectListItem,
  ResourceAdmin,
  ResourceListItem,
  TermAdmin,
  UserAdmin,
} from './admin.models';
import { AdminProject } from './admin-project.service';

export type Query = Readonly<Record<string, string | number | boolean | null | undefined>>;

/** Typed CRUD calls for one collection; the base URL is resolved per call (project may change). */
export class Collection<TList, TItem, TRequest> {
  constructor(
    private readonly http: HttpClient,
    private readonly base: () => string,
  ) {}

  list(query: Query = {}): Observable<Paged<TList>> {
    return this.http.get<Paged<TList>>(this.base(), { params: toParams(query) });
  }

  get(id: string): Observable<TItem> {
    return this.http.get<TItem>(`${this.base()}/${encodeURIComponent(id)}`);
  }

  create(request: TRequest): Observable<TItem> {
    return this.http.post<TItem>(this.base(), request);
  }

  update(id: string, request: TRequest): Observable<TItem> {
    return this.http.put<TItem>(`${this.base()}/${encodeURIComponent(id)}`, request);
  }

  remove(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base()}/${encodeURIComponent(id)}`);
  }

  action(id: string, action: string, body: unknown = {}): Observable<TItem> {
    return this.http.post<TItem>(`${this.base()}/${encodeURIComponent(id)}/${action}`, body);
  }
}

export function toParams(query: Query): HttpParams {
  let params = new HttpParams();
  for (const [key, value] of Object.entries(query)) {
    if (value !== null && value !== undefined && value !== '') {
      params = params.set(key, String(value));
    }
  }
  return params;
}

/** Every CMS endpoint of the API (`/api/v1/admin/...`). */
@Injectable()
export class AdminApi {
  private readonly http = inject(HttpClient);
  private readonly project = inject(AdminProject);
  private readonly root = `${inject(APP_ENVIRONMENT).apiBaseUrl.replace(/\/+$/, '')}/admin`;

  private projectUrl(path = ''): string {
    return `${this.root}/projects/${encodeURIComponent(this.project.slug())}${path ? `/${path}` : ''}`;
  }

  private collection<TList, TItem, TRequest>(base: () => string) {
    return new Collection<TList, TItem, TRequest>(this.http, base);
  }

  // Project-scoped content
  readonly news = this.collection<NewsListItem, NewsAdmin, unknown>(() => this.projectUrl('news'));
  readonly events = this.collection<EventListItem, EventAdmin, unknown>(() => this.projectUrl('events'));
  readonly resources = this.collection<ResourceListItem, ResourceAdmin, unknown>(() => this.projectUrl('resources'));
  readonly activities = this.collection<ActivityListItem, ActivityAdmin, unknown>(() => this.projectUrl('school-activities'));

  // Global content
  readonly people = this.collection<PersonListItem, PersonAdmin, unknown>(() => `${this.root}/people`);
  readonly media = this.collection<MediaAdmin, MediaAdmin, unknown>(() => `${this.root}/media`);

  // Projects
  projects(): Observable<ProjectListItem[]> {
    return this.http.get<ProjectListItem[]>(`${this.root}/projects`);
  }

  projectSettings(): Observable<ProjectAdmin> {
    return this.http.get<ProjectAdmin>(this.projectUrl());
  }

  updateProjectSettings(request: unknown): Observable<ProjectAdmin> {
    return this.http.put<ProjectAdmin>(this.projectUrl(), request);
  }

  dashboard(): Observable<Dashboard> {
    return this.http.get<Dashboard>(this.projectUrl('dashboard'));
  }

  lookups(): Observable<Lookups> {
    return this.http.get<Lookups>(this.projectUrl('lookups'), { params: { lang: 'ka' } });
  }

  // Pages
  pages(): Observable<PageListItem[]> {
    return this.http.get<PageListItem[]>(this.projectUrl('pages'));
  }

  page(key: string): Observable<PageAdmin> {
    return this.http.get<PageAdmin>(this.projectUrl(`pages/${encodeURIComponent(key)}`));
  }

  updatePage(key: string, request: unknown): Observable<PageAdmin> {
    return this.http.put<PageAdmin>(this.projectUrl(`pages/${encodeURIComponent(key)}`), request);
  }

  // Milestones
  milestones(): Observable<MilestoneAdmin[]> {
    return this.http.get<MilestoneAdmin[]>(this.projectUrl('milestones'));
  }

  createMilestone(request: unknown): Observable<MilestoneAdmin> {
    return this.http.post<MilestoneAdmin>(this.projectUrl('milestones'), request);
  }

  updateMilestone(id: string, request: unknown): Observable<MilestoneAdmin> {
    return this.http.put<MilestoneAdmin>(this.projectUrl(`milestones/${id}`), request);
  }

  reorderMilestones(ids: readonly string[]): Observable<MilestoneAdmin[]> {
    return this.http.put<MilestoneAdmin[]>(this.projectUrl('milestones/order'), { ids });
  }

  deleteMilestone(id: string): Observable<void> {
    return this.http.delete<void>(this.projectUrl(`milestones/${id}`));
  }

  // People roles
  updatePersonRoles(id: string, roles: unknown[], concurrencyStamp: string): Observable<PersonAdmin> {
    return this.http.put<PersonAdmin>(`${this.root}/people/${id}/roles`, { roles, concurrencyStamp });
  }

  // Organizations
  organizations(): Observable<OrganizationAdmin[]> {
    return this.http.get<OrganizationAdmin[]>(`${this.root}/organizations`);
  }

  organization(id: string): Observable<OrganizationAdmin> {
    return this.http.get<OrganizationAdmin>(`${this.root}/organizations/${id}`);
  }

  createOrganization(request: unknown): Observable<OrganizationAdmin> {
    return this.http.post<OrganizationAdmin>(`${this.root}/organizations`, request);
  }

  updateOrganization(id: string, request: unknown): Observable<OrganizationAdmin> {
    return this.http.put<OrganizationAdmin>(`${this.root}/organizations/${id}`, request);
  }

  deleteOrganization(id: string): Observable<void> {
    return this.http.delete<void>(`${this.root}/organizations/${id}`);
  }

  // Taxonomy: kind is `tags` or `disciplines`
  terms(kind: TermKind): Observable<TermAdmin[]> {
    return this.http.get<TermAdmin[]>(`${this.root}/${kind}`);
  }

  createTerm(kind: TermKind, request: unknown): Observable<TermAdmin> {
    return this.http.post<TermAdmin>(`${this.root}/${kind}`, request);
  }

  updateTerm(kind: TermKind, id: string, request: unknown): Observable<TermAdmin> {
    return this.http.put<TermAdmin>(`${this.root}/${kind}/${id}`, request);
  }

  deleteTerm(kind: TermKind, id: string): Observable<void> {
    return this.http.delete<void>(`${this.root}/${kind}/${id}`);
  }

  // Media
  upload(file: File, meta: Readonly<Record<string, string>>): Observable<MediaAdmin> {
    const form = new FormData();
    form.append('file', file, file.name);
    for (const [key, value] of Object.entries(meta)) {
      if (value) {
        form.append(key, value);
      }
    }
    return this.http.post<MediaAdmin>(`${this.root}/media`, form);
  }

  mediaUsages(id: string): Observable<MediaUsage[]> {
    return this.http.get<MediaUsage[]>(`${this.root}/media/${id}/usages`);
  }

  // Users & audit (Admin)
  users(): Observable<UserAdmin[]> {
    return this.http.get<UserAdmin[]>(`${this.root}/users`);
  }

  createUser(request: unknown): Observable<UserAdmin> {
    return this.http.post<UserAdmin>(`${this.root}/users`, request);
  }

  updateUser(id: string, request: unknown): Observable<UserAdmin> {
    return this.http.put<UserAdmin>(`${this.root}/users/${encodeURIComponent(id)}`, request);
  }

  resetPassword(id: string, newPassword: string): Observable<void> {
    return this.http.post<void>(`${this.root}/users/${encodeURIComponent(id)}/reset-password`, { newPassword });
  }

  unlockUser(id: string): Observable<void> {
    return this.http.post<void>(`${this.root}/users/${encodeURIComponent(id)}/unlock`, {});
  }

  audit(query: Query): Observable<Paged<AuditEntry>> {
    return this.http.get<Paged<AuditEntry>>(`${this.root}/audit`, { params: toParams(query) });
  }
}

export type TermKind = 'tags' | 'disciplines';
