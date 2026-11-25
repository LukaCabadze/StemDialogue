import { Directive, computed, effect, inject, input, signal, untracked } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, switchMap } from 'rxjs';

import { AdminApi } from './admin-api.service';
import { AdminAuth } from './admin-auth.service';
import { AdminLookups } from './admin-lookups.service';
import { AdminProject } from './admin-project.service';
import { AuditInfo } from './admin.models';
import { ProblemView, toProblem } from './problem';
import { Toasts } from './toasts.service';

/** Implemented by pages that can hold unsaved edits (checked before leaving the page). */
export interface HasUnsavedChanges {
  dirty(): boolean;
}

/**
 * Shared behaviour of every CMS editor: load by `:id` (or `new`), keep a mutable form model,
 * save with the concurrency stamp, run lifecycle actions and report validation problems.
 */
@Directive()
export abstract class EditorPage<TItem extends { readonly id: string; readonly audit: AuditInfo }, TForm extends object>
  implements HasUnsavedChanges
{
  protected readonly api = inject(AdminApi);
  protected readonly lookups = inject(AdminLookups);
  protected readonly toasts = inject(Toasts);
  protected readonly router = inject(Router);
  protected readonly auth = inject(AdminAuth);
  protected readonly project = inject(AdminProject);

  /** Route parameter: an id, or `new`. */
  readonly id = input<string>('new');

  readonly item = signal<TItem | null>(null);
  readonly form = signal<TForm | null>(null);
  readonly loading = signal(false);
  readonly busy = signal(false);
  readonly problem = signal<ProblemView | null>(null);
  readonly isNew = computed(() => this.id() === 'new');

  private snapshot = '';

  /** List page the editor returns to, relative to the admin root, e.g. `news`. */
  protected abstract readonly listPath: string;

  protected get listUrl(): string {
    return this.project.url(this.listPath);
  }
  protected abstract fetch(id: string): Observable<TItem>;
  protected abstract toForm(item: TItem | null): TForm;
  protected abstract toRequest(form: TForm): object;
  protected abstract createItem(request: object): Observable<TItem>;
  protected abstract updateItem(id: string, request: object): Observable<TItem>;

  /** Hook after a successful save (e.g. refresh lookups that list this entity). */
  protected afterSave(): void {}

  constructor() {
    effect(() => {
      const id = this.id();
      untracked(() => this.load(id));
    });
  }

  dirty(): boolean {
    const form = this.form();
    return form !== null && !this.busy() && JSON.stringify(this.toRequest(form)) !== this.snapshot;
  }

  load(id: string): void {
    this.problem.set(null);
    if (id === 'new') {
      this.accept(null);
      return;
    }
    this.loading.set(true);
    this.fetch(id).subscribe({
      next: (item) => {
        this.accept(item);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.fail(error);
      },
    });
  }

  save(): void {
    this.persist().subscribe({ next: () => undefined, error: () => undefined });
  }

  /** Runs a lifecycle action (publish, complete …), saving pending edits first. */
  protected runAction(action: (item: TItem) => Observable<TItem>, message: string): void {
    const pending = this.dirty() || this.isNew() ? this.persist() : null;
    const current = this.item();
    if (!pending && !current) {
      return;
    }
    this.busy.set(true);
    const run = pending
      ? pending.pipe(
          switchMap((saved) => {
            this.busy.set(true); // the save just finished; stay busy while the action runs
            return action(saved);
          }),
        )
      : action(current!);
    run.subscribe({
      next: (item) => {
        this.busy.set(false);
        this.accept(item);
        this.afterSave();
        this.toasts.success(message);
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.fail(error);
      },
    });
  }

  protected deleteItem(remove: (id: string) => Observable<void>, question: string): void {
    const current = this.item();
    if (!current || !confirm(question)) {
      return;
    }
    this.busy.set(true);
    remove(current.id).subscribe({
      next: () => {
        this.busy.set(false);
        this.snapshot = JSON.stringify(this.toRequest(this.form()!));
        this.afterSave();
        this.toasts.success('წაიშალა');
        void this.router.navigateByUrl(this.listUrl);
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.fail(error);
      },
    });
  }

  protected accept(item: TItem | null): void {
    this.item.set(item);
    const form = this.toForm(item);
    this.form.set(form);
    this.snapshot = JSON.stringify(this.toRequest(form));
  }

  protected fail(error: unknown): void {
    const problem = toProblem(error);
    this.problem.set(problem);
    this.toasts.error(problem.title);
  }

  private persist(): Observable<TItem> {
    return new Observable<TItem>((subscriber) => {
      const form = this.form();
      if (!form) {
        subscriber.complete();
        return;
      }
      const wasNew = this.isNew();
      const current = this.item();
      const request = { ...this.toRequest(form), concurrencyStamp: current?.audit.concurrencyStamp ?? null };
      this.busy.set(true);
      this.problem.set(null);
      (wasNew || !current ? this.createItem(request) : this.updateItem(current.id, request)).subscribe({
        next: (saved) => {
          this.busy.set(false);
          this.accept(saved);
          this.afterSave();
          this.toasts.success('შენახულია');
          if (wasNew) {
            void this.router.navigate([this.listUrl, saved.id], { replaceUrl: true });
          }
          subscriber.next(saved);
          subscriber.complete();
        },
        error: (error: unknown) => {
          this.busy.set(false);
          this.fail(error);
          subscriber.error(error);
        },
      });
    });
  }
}
