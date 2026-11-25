import { Directive, effect, inject, signal, untracked } from '@angular/core';
import { Observable } from 'rxjs';

import { AdminApi } from './admin-api.service';
import { AdminLookups } from './admin-lookups.service';
import { AdminProject } from './admin-project.service';
import { Paged } from './admin.models';
import { formatDate, formatUtc } from './admin-time';
import { ProblemView, toProblem } from './problem';

/** Paged CMS list: reloads on project change, keeps filters in signals bound to the toolbar. */
@Directive()
export abstract class ListPage<T> {
  protected readonly api = inject(AdminApi);
  protected readonly lookups = inject(AdminLookups);
  protected readonly project = inject(AdminProject);

  readonly result = signal<Paged<T> | null>(null);
  readonly loading = signal(false);
  readonly problem = signal<ProblemView | null>(null);
  protected page = 1;

  protected abstract fetch(page: number): Observable<Paged<T>>;

  constructor() {
    effect(() => {
      this.project.slug();
      untracked(() => this.reload());
    });
  }

  reload(): void {
    this.page = 1;
    this.load();
  }

  goTo(page: number): void {
    this.page = page;
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.problem.set(null);
    this.fetch(this.page).subscribe({
      next: (result) => {
        this.result.set(result);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.problem.set(toProblem(error));
      },
    });
  }

  protected when(utc: string | null | undefined): string {
    return formatUtc(utc);
  }

  protected day(date: string | null | undefined): string {
    return formatDate(date);
  }
}
