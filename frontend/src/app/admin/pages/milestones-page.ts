import { ChangeDetectionStrategy, Component, effect, inject, signal, untracked } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';

import { AdminApi } from '../core/admin-api.service';
import { AdminProject } from '../core/admin-project.service';
import { LText, MilestoneAdmin, lt } from '../core/admin.models';
import { formatDate, orNull } from '../core/admin-time';
import { ProblemView, toProblem } from '../core/problem';
import { Toasts } from '../core/toasts.service';
import { LTextField } from '../ui/ltext-field';
import { ProblemBox } from '../ui/widgets';

interface MilestoneForm {
  title: LText;
  description: LText;
  periodLabel: LText;
  location: LText;
  startDate: string;
  endDate: string;
  isVisible: boolean;
}

/** Project stages shown on the About page and the home teaser; ordered by drag-free up/down buttons. */
@Component({
  selector: 'adm-milestones-page',
  imports: [FormsModule, LTextField, ProblemBox],
  template: `
    <header class="adm-page-head">
      <h1>პროექტის ეტაპები</h1>
      <div class="adm-page-head__actions">
        <button type="button" class="adm-btn adm-btn--primary" (click)="startNew()">+ ახალი ეტაპი</button>
      </div>
    </header>
    <p class="adm-lead">ჩანს „პროექტის შესახებ“ გვერდზე და მთავარ გვერდზე. სტატუსი (მომავალი / მიმდინარე / დასრულებული) ითვლება თარიღებიდან.</p>
    <adm-problem [problem]="problem()" />

    @if (editing(); as e) {
      <section class="adm-card">
        <h2 class="adm-card__title">{{ e.id ? 'ეტაპის რედაქტირება' : 'ახალი ეტაპი' }}</h2>
        <adm-ltext label="სათაური" [value]="e.form.title" [required]="true" [maxLength]="200" />
        <adm-ltext label="აღწერა" kind="textarea" [value]="e.form.description" [maxLength]="1000" />
        <div class="adm-grid adm-grid--2">
          <adm-ltext label="პერიოდის წარწერა" [value]="e.form.periodLabel" [maxLength]="80" hint="მაგ.: 2027 წლის მაისი" />
          <adm-ltext label="ადგილი" [value]="e.form.location" [maxLength]="150" />
        </div>
        <div class="adm-grid adm-grid--3">
          <label class="adm-field"><span class="adm-field__label">დაწყება</span><input type="date" [(ngModel)]="e.form.startDate" /></label>
          <label class="adm-field"><span class="adm-field__label">დასრულება</span><input type="date" [(ngModel)]="e.form.endDate" /></label>
          <label class="adm-check adm-field--end"><input type="checkbox" [(ngModel)]="e.form.isVisible" /><span>საიტზე ჩანს</span></label>
        </div>
        <div class="adm-page-head__actions">
          <button type="button" class="adm-btn adm-btn--primary" [disabled]="busy()" (click)="save()">შენახვა</button>
          <button type="button" class="adm-btn adm-btn--ghost" (click)="editing.set(null)">გაუქმება</button>
        </div>
      </section>
    }

    <div class="adm-table-wrap">
      <table class="adm-table">
        <thead>
          <tr><th>#</th><th>ეტაპი</th><th>პერიოდი</th><th>ჩანს</th><th></th></tr>
        </thead>
        <tbody>
          @for (m of items(); track m.id; let i = $index, first = $first, last = $last) {
            <tr>
              <td>{{ i + 1 }}</td>
              <td><b>{{ m.title.ka }}</b><small class="adm-hint">{{ m.location.ka }}</small></td>
              <td>{{ m.periodLabel.ka || range(m) }}</td>
              <td>{{ m.isVisible ? '✓' : '—' }}</td>
              <td style="white-space: nowrap">
                <button type="button" class="adm-icon-btn" [disabled]="first || busy()" aria-label="ზემოთ" (click)="move(i, -1)">↑</button>
                <button type="button" class="adm-icon-btn" [disabled]="last || busy()" aria-label="ქვემოთ" (click)="move(i, 1)">↓</button>
                <button type="button" class="adm-btn adm-btn--sm" (click)="edit(m)">რედაქტირება</button>
                <button type="button" class="adm-icon-btn adm-icon-btn--danger" aria-label="წაშლა" [disabled]="busy()" (click)="remove(m)">✕</button>
              </td>
            </tr>
          } @empty {
            <tr><td colspan="5" class="adm-empty">ეტაპები ჯერ არ არის.</td></tr>
          }
        </tbody>
      </table>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.Default,
})
export class MilestonesPage {
  private readonly api = inject(AdminApi);
  private readonly project = inject(AdminProject);
  private readonly toasts = inject(Toasts);

  protected readonly items = signal<MilestoneAdmin[]>([]);
  protected readonly editing = signal<{ id: string | null; stamp: string | null; form: MilestoneForm } | null>(null);
  protected readonly busy = signal(false);
  protected readonly problem = signal<ProblemView | null>(null);

  constructor() {
    effect(() => {
      this.project.slug();
      untracked(() => this.load());
    });
  }

  protected startNew(): void {
    this.editing.set({
      id: null,
      stamp: null,
      form: { title: lt(null), description: lt(null), periodLabel: lt(null), location: lt(null), startDate: '', endDate: '', isVisible: true },
    });
  }

  protected edit(m: MilestoneAdmin): void {
    this.editing.set({
      id: m.id,
      stamp: m.audit.concurrencyStamp,
      form: {
        title: lt(m.title),
        description: lt(m.description),
        periodLabel: lt(m.periodLabel),
        location: lt(m.location),
        startDate: m.startDate ?? '',
        endDate: m.endDate ?? '',
        isVisible: m.isVisible,
      },
    });
  }

  protected save(): void {
    const e = this.editing();
    if (!e) {
      return;
    }
    const request = {
      ...e.form,
      startDate: orNull(e.form.startDate),
      endDate: orNull(e.form.endDate),
      concurrencyStamp: e.stamp,
    };
    this.run(e.id ? this.api.updateMilestone(e.id, request) : this.api.createMilestone(request), () => {
      this.editing.set(null);
      this.toasts.success('შენახულია');
      this.load();
    });
  }

  protected move(index: number, delta: number): void {
    const ids = this.items().map((m) => m.id);
    const [id] = ids.splice(index, 1);
    ids.splice(index + delta, 0, id);
    this.run(this.api.reorderMilestones(ids), (items) => this.items.set(items as MilestoneAdmin[]));
  }

  protected remove(m: MilestoneAdmin): void {
    if (confirm(`წავშალოთ ეტაპი „${m.title.ka}“?`)) {
      this.run(this.api.deleteMilestone(m.id), () => {
        this.toasts.success('წაიშალა');
        this.load();
      });
    }
  }

  protected range(m: MilestoneAdmin): string {
    return m.startDate || m.endDate ? `${formatDate(m.startDate)} – ${formatDate(m.endDate)}` : '—';
  }

  private load(): void {
    this.api.milestones().subscribe({ next: (items) => this.items.set(items), error: (e: unknown) => this.problem.set(toProblem(e)) });
  }

  private run<T>(call: Observable<T>, done: (value: T) => void): void {
    this.busy.set(true);
    this.problem.set(null);
    call.subscribe({
      next: (value) => {
        this.busy.set(false);
        done(value);
      },
      error: (error: unknown) => {
        this.busy.set(false);
        const problem = toProblem(error);
        this.problem.set(problem);
        this.toasts.error(problem.title);
      },
    });
  }
}
