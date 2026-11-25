import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { AdminApi, TermKind } from '../core/admin-api.service';
import { AdminLookups } from '../core/admin-lookups.service';
import { TermAdmin } from '../core/admin.models';
import { orNull } from '../core/admin-time';
import { ProblemView, toProblem } from '../core/problem';
import { Toasts } from '../core/toasts.service';
import { ProblemBox } from '../ui/widgets';

interface TermDraft {
  id: string | null;
  ka: string;
  en: string;
  slug: string;
  displayOrder: number;
}

const emptyDraft = (): TermDraft => ({ id: null, ka: '', en: '', slug: '', displayOrder: 0 });

@Component({
  selector: 'adm-taxonomy-page',
  imports: [FormsModule, ProblemBox],
  template: `
    <header class="adm-page-head"><h1>თეგები და მიმართულებები</h1></header>
    <p class="adm-lead">
      <b>მიმართულებები</b> (ფიზიკა, ქიმია, ბიოლოგია…) — STEM შაბათობების, მკვლევრებისა და რესურსების ფილტრი.
      <b>თეგები</b> — თავისუფალი თემები სიახლეებისთვის, ღონისძიებებისა და რესურსებისთვის.
    </p>
    <adm-problem [problem]="problem()" />

    <div class="adm-grid adm-grid--2">
      @for (kind of kinds; track kind.key) {
        <section class="adm-card">
          <h2 class="adm-card__title">{{ kind.title }}</h2>
          <div class="adm-row">
            <div class="adm-row__line">
              <input type="text" placeholder="ქართულად *" [(ngModel)]="drafts[kind.key].ka" aria-label="სახელი ქართულად" />
              <input type="text" placeholder="English" [(ngModel)]="drafts[kind.key].en" aria-label="Name in English" />
            </div>
            <div class="adm-row__line">
              <input type="text" placeholder="slug (არასავალდ.)" [(ngModel)]="drafts[kind.key].slug" aria-label="slug" />
              <input type="number" style="flex: 0 0 80px" [(ngModel)]="drafts[kind.key].displayOrder" aria-label="რიგი" />
              <button type="button" class="adm-btn adm-btn--primary adm-btn--sm" [disabled]="!drafts[kind.key].ka.trim() || busy()" (click)="save(kind.key)">
                {{ drafts[kind.key].id ? 'შენახვა' : 'დამატება' }}
              </button>
              @if (drafts[kind.key].id) {
                <button type="button" class="adm-btn adm-btn--ghost adm-btn--sm" (click)="drafts[kind.key] = empty()">გაუქმება</button>
              }
            </div>
          </div>
          <table class="adm-table">
            <thead><tr><th>სახელი</th><th>slug</th><th>გამოყ.</th><th></th></tr></thead>
            <tbody>
              @for (t of terms()[kind.key]; track t.id) {
                <tr>
                  <td>{{ t.name.ka }}<small class="adm-hint">{{ t.name.en }}</small></td>
                  <td><code>{{ t.slug }}</code></td>
                  <td>{{ t.usageCount }}</td>
                  <td style="white-space: nowrap">
                    <button type="button" class="adm-btn adm-btn--sm" (click)="edit(kind.key, t)">შეცვლა</button>
                    <button type="button" class="adm-icon-btn adm-icon-btn--danger" aria-label="წაშლა" (click)="remove(kind.key, t)">✕</button>
                  </td>
                </tr>
              } @empty {
                <tr><td colspan="4" class="adm-empty">ჯერ არაფერია.</td></tr>
              }
            </tbody>
          </table>
        </section>
      }
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.Default,
})
export class TaxonomyPage {
  private readonly api = inject(AdminApi);
  private readonly lookups = inject(AdminLookups);
  private readonly toasts = inject(Toasts);

  protected readonly kinds: readonly { key: TermKind; title: string }[] = [
    { key: 'disciplines', title: 'მიმართულებები' },
    { key: 'tags', title: 'თეგები' },
  ];
  protected readonly terms = signal<Record<TermKind, TermAdmin[]>>({ tags: [], disciplines: [] });
  protected readonly busy = signal(false);
  protected readonly problem = signal<ProblemView | null>(null);
  protected drafts: Record<TermKind, TermDraft> = { tags: emptyDraft(), disciplines: emptyDraft() };
  protected readonly empty = emptyDraft;

  constructor() {
    this.load('tags');
    this.load('disciplines');
  }

  protected edit(kind: TermKind, t: TermAdmin): void {
    this.drafts[kind] = { id: t.id, ka: t.name.ka ?? '', en: t.name.en ?? '', slug: t.slug, displayOrder: t.displayOrder };
  }

  protected save(kind: TermKind): void {
    const d = this.drafts[kind];
    const request = { name: { ka: d.ka, en: orNull(d.en) }, slug: orNull(d.slug), displayOrder: d.displayOrder ?? 0 };
    this.busy.set(true);
    (d.id ? this.api.updateTerm(kind, d.id, request) : this.api.createTerm(kind, request)).subscribe({
      next: () => {
        this.busy.set(false);
        this.drafts[kind] = emptyDraft();
        this.toasts.success('შენახულია');
        this.load(kind);
        this.lookups.reload();
      },
      error: (error: unknown) => this.fail(error),
    });
  }

  protected remove(kind: TermKind, t: TermAdmin): void {
    const warning = t.usageCount ? ` ის მოიხსნება ${t.usageCount} ჩანაწერიდან.` : '';
    if (!confirm(`წავშალოთ „${t.name.ka}“?${warning}`)) {
      return;
    }
    this.api.deleteTerm(kind, t.id).subscribe({
      next: () => {
        this.toasts.success('წაიშალა');
        this.load(kind);
        this.lookups.reload();
      },
      error: (error: unknown) => this.fail(error),
    });
  }

  private load(kind: TermKind): void {
    this.api.terms(kind).subscribe({
      next: (items) => this.terms.update((all) => ({ ...all, [kind]: items })),
      error: (error: unknown) => this.problem.set(toProblem(error)),
    });
  }

  private fail(error: unknown): void {
    this.busy.set(false);
    const p = toProblem(error);
    this.problem.set(p);
    this.toasts.error(p.title);
  }
}
