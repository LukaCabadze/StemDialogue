import { ChangeDetectionStrategy, Component, inject, input, linkedSignal, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { LText, MediaAdmin, MediaUsage, Paged, lt } from '../core/admin.models';
import { formatBytes, formatUtc } from '../core/admin-time';
import { ListPage } from '../core/list-page';
import { toProblem } from '../core/problem';
import { Toasts } from '../core/toasts.service';
import { LTextField } from '../ui/ltext-field';
import { Pager, ProblemBox } from '../ui/widgets';

interface Selected {
  media: MediaAdmin;
  altText: LText;
  caption: LText;
  credit: LText;
  usages: MediaUsage[] | null;
}

@Component({
  selector: 'adm-media-page',
  imports: [FormsModule, LTextField, Pager, ProblemBox],
  template: `
    <header class="adm-page-head"><h1>მედია ფაილები</h1></header>

    <section class="adm-upload">
      <h3>ატვირთვა</h3>
      <div class="adm-grid adm-grid--3">
        <label class="adm-field">
          <span class="adm-field__label">ფაილ(ებ)ი</span>
          <input type="file" multiple (change)="onFiles($event)" />
        </label>
        <label class="adm-field">
          <span class="adm-field__label">Alt ტექსტი (ქართ.) — სურათებისთვის</span>
          <input type="text" [(ngModel)]="altKa" maxlength="250" />
        </label>
        <div class="adm-field adm-field--end">
          <button type="button" class="adm-btn adm-btn--primary" [disabled]="!files().length || uploading()" (click)="upload()">
            {{ uploading() ? 'იტვირთება…' : 'ატვირთვა (' + files().length + ')' }}
          </button>
        </div>
      </div>
      <p class="adm-hint">სურათები: JPG, PNG, WebP, GIF, SVG (10 MB). დოკუმენტები: PDF, PPTX, PPSX, DOCX, XLSX (50 MB). ვიდეო — YouTube/Vimeo ბმულით რესურსებში.</p>
    </section>

    <div class="adm-toolbar">
      <input type="search" placeholder="ძებნა…" [(ngModel)]="search" (keyup.enter)="reload()" />
      <select [(ngModel)]="kind" (ngModelChange)="reload()" aria-label="ტიპი">
        <option value="">ყველა ტიპი</option>
        <option value="Image">სურათები</option>
        <option value="Document">დოკუმენტები</option>
        <option value="Presentation">პრეზენტაციები</option>
        <option value="Spreadsheet">ცხრილები</option>
      </select>
      <label class="adm-check"><input type="checkbox" [(ngModel)]="onlyMissingAlt" /><span>მხოლოდ Alt-ის გარეშე</span></label>
    </div>
    <adm-problem [problem]="problem()" />

    <div class="adm-editor">
      <div>
        <div class="adm-media-grid">
          @for (m of visible(); track m.id) {
            <button type="button" class="adm-media-tile" [class.is-selected]="selected()?.media?.id === m.id" (click)="select(m)">
              @if (m.kind === 'Image') {
                <img [src]="m.url" [alt]="m.altText.ka ?? ''" loading="lazy" />
              } @else {
                <span class="adm-media-tile__doc">{{ ext(m.originalFileName) }}</span>
              }
              <span class="adm-media-tile__name">{{ m.originalFileName }}</span>
              <span class="adm-media-tile__meta">{{ size(m.sizeBytes) }}@if (m.missingAltText) { · <b class="adm-warn">alt არ აქვს</b>}</span>
            </button>
          } @empty {
            <p class="adm-empty">{{ loading() ? 'იტვირთება…' : 'ფაილები ვერ მოიძებნა.' }}</p>
          }
        </div>
        @if (result(); as r) {
          <adm-pager [page]="r.page" [totalPages]="r.totalPages" [total]="r.total" (pageChange)="goTo($event)" />
        }
      </div>

      <aside class="adm-editor__side">
        @if (selected(); as s) {
          <section class="adm-card">
            @if (s.media.kind === 'Image') {
              <img [src]="s.media.url" [alt]="s.altText.ka ?? ''" style="width: 100%; border-radius: 6px; margin-bottom: 12px" />
            }
            <p><b>{{ s.media.originalFileName }}</b></p>
            <p class="adm-hint">
              {{ size(s.media.sizeBytes) }} · {{ s.media.contentType }}
              @if (s.media.width) { · {{ s.media.width }}×{{ s.media.height }} }
              <br />ატვირთა {{ s.media.audit.createdBy ?? '—' }}, {{ when(s.media.audit.createdAtUtc) }}
            </p>
            <p><a [href]="s.media.url" target="_blank" rel="noopener">გახსნა ↗</a> · <button type="button" class="adm-btn adm-btn--sm adm-btn--ghost" (click)="copy(s.media.url)">ბმულის კოპირება</button></p>

            <adm-ltext label="Alt ტექსტი" [value]="s.altText" [maxLength]="250" hint="რა არის გამოსახული — ეკრანის მკითხველისთვის." />
            <adm-ltext label="წარწერა" [value]="s.caption" [maxLength]="500" />
            <adm-ltext label="ავტორი / წყარო" [value]="s.credit" [maxLength]="200" />
            <button type="button" class="adm-btn adm-btn--primary adm-btn--block" [disabled]="busy()" (click)="saveMeta(s)">შენახვა</button>

            <h3 class="adm-card__title" style="margin-top: 16px">სად გამოიყენება</h3>
            @if (s.usages === null) {
              <p class="adm-hint">იტვირთება…</p>
            } @else {
              @for (u of s.usages; track u.id + u.contentType) {
                <p class="adm-hint">{{ u.contentType }}: {{ u.title }}</p>
              } @empty {
                <p class="adm-hint">არსად — შეიძლება წაშლა.</p>
              }
            }
            <button type="button" class="adm-btn adm-btn--danger adm-btn--block" [disabled]="busy() || !!s.usages?.length" (click)="remove(s)">ფაილის წაშლა</button>
          </section>
        } @else {
          <section class="adm-card"><p class="adm-hint">აირჩიეთ ფაილი Alt ტექსტის, წარწერის ან წაშლისთვის.</p></section>
        }
      </aside>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.Default,
})
export class MediaPage extends ListPage<MediaAdmin> {
  private readonly toasts = inject(Toasts);
  readonly kindParam = input<string | undefined>(undefined, { alias: 'kind' });
  protected readonly kind = linkedSignal(() => this.kindParam() ?? '');
  protected search = '';
  protected onlyMissingAlt = false;
  protected altKa = '';
  protected readonly files = signal<File[]>([]);
  protected readonly uploading = signal(false);
  protected readonly busy = signal(false);
  protected readonly selected = signal<Selected | null>(null);

  protected fetch(page: number) {
    return this.api.media.list({ page, pageSize: 30, kind: this.kind(), search: this.search });
  }

  protected visible(): MediaAdmin[] {
    const items = this.result()?.items ?? [];
    return this.onlyMissingAlt ? items.filter((m) => m.missingAltText) : items;
  }

  protected onFiles(event: Event): void {
    this.files.set(Array.from((event.target as HTMLInputElement).files ?? []));
  }

  protected upload(): void {
    const files = this.files();
    this.uploading.set(true);
    let done = 0;
    let failed = 0;
    const finish = () => {
      if (done + failed === files.length) {
        this.uploading.set(false);
        this.files.set([]);
        this.altKa = '';
        if (done) {
          this.toasts.success(`აიტვირთა ${done} ფაილი`);
        }
        this.reload();
      }
    };
    for (const file of files) {
      this.api.upload(file, { altKa: this.altKa }).subscribe({
        next: () => {
          done++;
          finish();
        },
        error: (error: unknown) => {
          failed++;
          const p = toProblem(error);
          this.toasts.error(`${file.name}: ${p.detail ?? p.title} ${p.fields.flatMap((f) => f.messages).join(' ')}`);
          finish();
        },
      });
    }
  }

  protected select(media: MediaAdmin): void {
    const selected: Selected = { media, altText: lt(media.altText), caption: lt(media.caption), credit: lt(media.credit), usages: null };
    this.selected.set(selected);
    this.api.mediaUsages(media.id).subscribe((usages) => {
      if (this.selected() === selected) {
        this.selected.set({ ...selected, usages });
      }
    });
  }

  protected saveMeta(s: Selected): void {
    this.busy.set(true);
    this.api.media
      .update(s.media.id, { altText: s.altText, caption: s.caption, credit: s.credit, concurrencyStamp: s.media.audit.concurrencyStamp })
      .subscribe({
        next: (media) => {
          this.busy.set(false);
          this.toasts.success('შენახულია');
          this.selected.set({ ...s, media });
          this.result.update((r) => (r ? ({ ...r, items: r.items.map((m) => (m.id === media.id ? media : m)) } as Paged<MediaAdmin>) : r));
        },
        error: (error: unknown) => this.failed(error),
      });
  }

  protected remove(s: Selected): void {
    if (!confirm(`წავშალოთ „${s.media.originalFileName}“?`)) {
      return;
    }
    this.busy.set(true);
    this.api.media.remove(s.media.id).subscribe({
      next: () => {
        this.busy.set(false);
        this.selected.set(null);
        this.toasts.success('წაიშალა');
        this.load();
      },
      error: (error: unknown) => this.failed(error),
    });
  }

  protected copy(url: string): void {
    void navigator.clipboard?.writeText(new URL(url, location.origin).toString()).then(() => this.toasts.info('ბმული დაკოპირდა'));
  }

  protected size(bytes: number): string {
    return formatBytes(bytes);
  }

  protected ext(name: string): string {
    return name.split('.').pop()?.toUpperCase() ?? 'FILE';
  }

  protected override when(utc: string | null | undefined): string {
    return formatUtc(utc);
  }

  private failed(error: unknown): void {
    this.busy.set(false);
    const p = toProblem(error);
    this.problem.set(p);
    this.toasts.error(p.title);
  }
}
