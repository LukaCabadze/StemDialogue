import { ChangeDetectionStrategy, Component, computed, inject, input, linkedSignal, model, output } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { AdminLookups } from '../core/admin-lookups.service';
import { LookupItem } from '../core/admin.models';
import { ProblemView } from '../core/problem';

/** Multi-select of tags, disciplines… shown as toggle chips with a filter when the list is long. */
@Component({
  selector: 'adm-check-list',
  imports: [FormsModule],
  template: `
    <div class="adm-field">
      <span class="adm-field__label">{{ label() }}</span>
      @if (options().length > 12) {
        <input type="search" class="adm-chips__filter" placeholder="ფილტრი…" [(ngModel)]="filter" />
      }
      <div class="adm-chips" role="group" [attr.aria-label]="label()">
        @for (option of visible(); track option.id) {
          <button type="button" class="adm-chip" [class.is-on]="isOn(option.id)" [attr.aria-pressed]="isOn(option.id)" (click)="toggle(option.id)">
            {{ option.label }}
          </button>
        } @empty {
          <span class="adm-hint">{{ empty() }}</span>
        }
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.Default,
})
export class CheckList {
  readonly label = input.required<string>();
  readonly options = input.required<readonly LookupItem[]>();
  readonly selected = model<string[]>([]);
  readonly empty = input('ჯერ არაფერია დამატებული.');
  protected filter = '';

  protected visible(): readonly LookupItem[] {
    const text = this.filter.trim().toLowerCase();
    const options = this.options();
    return text ? options.filter((o) => o.label.toLowerCase().includes(text) || this.isOn(o.id)) : options;
  }

  protected isOn(id: string): boolean {
    return this.selected().includes(id);
  }

  protected toggle(id: string): void {
    this.selected.set(this.isOn(id) ? this.selected().filter((x) => x !== id) : [...this.selected(), id]);
  }
}

const tones: Readonly<Record<string, string>> = {
  Published: 'ok',
  Completed: 'ok',
  Draft: 'muted',
  Archived: 'muted',
  Planned: 'info',
  Postponed: 'warn',
  Cancelled: 'danger',
  Scheduled: 'warn',
};

/** Coloured status pill with the Georgian label from lookups. */
@Component({
  selector: 'adm-status',
  template: `<span class="adm-badge" [attr.data-tone]="tone()">{{ text() }}</span>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatusBadge {
  private readonly lookups = inject(AdminLookups);
  readonly value = input.required<string>();
  readonly label = input<string | null>(null);
  protected readonly tone = computed(() => tones[this.value()] ?? 'muted');
  protected readonly text = computed(() => this.label() ?? this.lookups.label(this.value()));
}

/** Page navigation for admin lists. */
@Component({
  selector: 'adm-pager',
  template: `
    @if (totalPages() > 1) {
      <nav class="adm-pager" aria-label="გვერდები">
        <button type="button" class="adm-btn adm-btn--sm" [disabled]="page() <= 1" (click)="pageChange.emit(page() - 1)">← წინა</button>
        <span>{{ page() }} / {{ totalPages() }} · სულ {{ total() }}</span>
        <button type="button" class="adm-btn adm-btn--sm" [disabled]="page() >= totalPages()" (click)="pageChange.emit(page() + 1)">შემდეგი →</button>
      </nav>
    } @else if (total() > 0) {
      <p class="adm-pager adm-hint">სულ {{ total() }}</p>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Pager {
  readonly page = input.required<number>();
  readonly totalPages = input.required<number>();
  readonly total = input.required<number>();
  readonly pageChange = output<number>();
}

const fieldNames: Readonly<Record<string, string>> = {
  title: 'სათაური',
  slug: 'Slug (URL)',
  summary: 'მოკლე აღწერა',
  body: 'ტექსტი',
  fullName: 'სახელი, გვარი',
  startsAtUtc: 'დაწყების დრო',
  endsAtUtc: 'დასრულების დრო',
  registrationUrl: 'რეგისტრაციის ბმული',
  streamUrl: 'ტრანსლაციის ბმული',
  recordingUrl: 'ჩანაწერის ბმული',
  externalUrl: 'გარე ბმული',
  coverImageId: 'მთავარი სურათი',
  galleryImageIds: 'გალერეა',
  email: 'ელფოსტა',
  password: 'პაროლი',
  newPassword: 'ახალი პაროლი',
  name: 'სახელი',
  websiteUrl: 'ვებსაიტი',
};

/** Error summary with per-field validation messages from the API. */
@Component({
  selector: 'adm-problem',
  template: `
    @if (problem(); as p) {
      <div class="adm-alert adm-alert--danger" role="alert">
        <strong>{{ p.title }}</strong>
        @if (p.detail) {
          <p>{{ p.detail }}</p>
        }
        @if (p.fields.length) {
          <ul>
            @for (f of p.fields; track f.field) {
              <li><b>{{ name(f.field) }}:</b> {{ f.messages.join(' ') }}</li>
            }
          </ul>
        }
      </div>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProblemBox {
  readonly problem = input<ProblemView | null>(null);

  protected name(field: string): string {
    const root = field.split(/[.[]/)[0];
    return fieldNames[root] ? field.replace(root, fieldNames[root]) : field;
  }
}

/** Card with a header that expands to show its content (secondary form sections). */
@Component({
  selector: 'adm-collapsible',
  template: `
    <section class="adm-card">
      <button type="button" class="adm-card__toggle" [attr.aria-expanded]="open()" (click)="open.set(!open())">
        <span>{{ title() }}</span><span aria-hidden="true">{{ open() ? '−' : '+' }}</span>
      </button>
      @if (open()) {
        <div class="adm-card__body"><ng-content /></div>
      }
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Collapsible {
  readonly title = input.required<string>();
  readonly startOpen = input(false);
  readonly open = linkedSignal(() => this.startOpen());
}
