import { ChangeDetectionStrategy, Component, ViewEncapsulation, effect, inject, signal, untracked } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { AdminApi } from '../core/admin-api.service';
import { AdminAuth } from '../core/admin-auth.service';
import { AdminLookups } from '../core/admin-lookups.service';
import { AdmUrlPipe, AdminProject } from '../core/admin-project.service';
import { ProjectListItem } from '../core/admin.models';
import { Toasts } from '../core/toasts.service';

interface NavLink {
  readonly path: string;
  readonly label: string;
  readonly exact?: boolean;
}

interface NavGroup {
  readonly title: string;
  readonly adminOnly?: boolean;
  readonly links: readonly NavLink[];
}

/** CMS frame: sidebar navigation, project switcher, account menu and notifications. */
@Component({
  selector: 'adm-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, AdmUrlPipe],
  templateUrl: './admin-shell.html',
  styleUrls: ['./admin-base.scss', './admin-forms.scss', './admin-layout.scss', './admin-widgets.scss'],
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminShell {
  protected readonly auth = inject(AdminAuth);
  protected readonly project = inject(AdminProject);
  protected readonly toasts = inject(Toasts);
  private readonly api = inject(AdminApi);
  private readonly lookups = inject(AdminLookups);
  private readonly router = inject(Router);

  protected readonly projects = signal<ProjectListItem[]>([]);
  protected readonly menuOpen = signal(false);

  protected readonly groups: readonly NavGroup[] = [
    { title: '', links: [{ path: '', label: 'მთავარი პანელი', exact: true }] },
    {
      title: 'კონტენტი',
      links: [
        { path: 'news', label: 'სიახლეები' },
        { path: 'events', label: 'STEM შაბათობები' },
        { path: 'people', label: 'ადამიანები' },
        { path: 'resources', label: 'რესურსები' },
        { path: 'activities', label: 'ელჩების აქტივობები' },
      ],
    },
    {
      title: 'საიტის სტრუქტურა',
      links: [
        { path: 'pages', label: 'გვერდები' },
        { path: 'milestones', label: 'პროექტის ეტაპები' },
      ],
    },
    {
      title: 'ბიბლიოთეკა',
      links: [
        { path: 'media', label: 'მედია ფაილები' },
        { path: 'taxonomy', label: 'თეგები და მიმართულებები' },
        { path: 'organizations', label: 'ორგანიზაციები' },
      ],
    },
    {
      title: 'ადმინისტრირება',
      adminOnly: true,
      links: [
        { path: 'project', label: 'პროექტი და დაფინანსება' },
        { path: 'users', label: 'მომხმარებლები' },
        { path: 'audit', label: 'ცვლილებების ისტორია' },
      ],
    },
  ];

  constructor() {
    this.api.projects().subscribe((projects) => {
      this.projects.set(projects);
      if (projects.length && !projects.some((p) => p.slug === this.project.slug())) {
        this.switchProject(projects[0].slug);
      }
    });

    effect(() => {
      this.project.slug();
      untracked(() => this.lookups.reload());
    });
  }

  protected switchProject(slug: string): void {
    void this.router.navigateByUrl(`/projects/${encodeURIComponent(slug)}/admin`);
  }

  protected publicUrl(): string {
    return this.project.publicUrl();
  }

  protected logout(): void {
    this.auth.logout().subscribe(() => void this.router.navigateByUrl(this.project.url('login')));
  }
}
