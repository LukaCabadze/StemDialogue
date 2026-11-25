import { ChangeDetectionStrategy, Component, ElementRef, afterNextRender, inject, input, output, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { AdminApi } from '../core/admin-api.service';
import { MediaAdmin, MediaRef, toMediaRef } from '../core/admin.models';
import { formatBytes } from '../core/admin-time';
import { toProblem } from '../core/problem';

/** Modal media library: search, pick an existing file, or upload a new one (with alt text). */
@Component({
  selector: 'adm-media-picker',
  imports: [FormsModule],
  template: `
    <dialog #dialog class="adm-dialog adm-dialog--wide" (close)="closed.emit()" (cancel)="closed.emit()">
      <header class="adm-dialog__head">
        <h2>{{ imagesOnly() ? 'სურათის არჩევა' : 'ფაილის არჩევა' }}</h2>
        <button type="button" class="adm-icon-btn" aria-label="დახურვა" (click)="close()">✕</button>
      </header>

      <section class="adm-upload">
        <h3>ახალი ფაილის ატვირთვა</h3>
        <div class="adm-grid adm-grid--3">
          <label class="adm-field">
            <span class="adm-field__label">ფაილი</span>
            <input type="file" [accept]="accept()" (change)="onFile($event)" />
          </label>
          <label class="adm-field">
            <span class="adm-field__label">Alt ტექსტი (ქართ.) @if (needsAlt()) {<span class="adm-req">*</span>}</span>
            <input type="text" [(ngModel)]="altKa" maxlength="250" placeholder="რა არის გამოსახული" />
          </label>
          <label class="adm-field">
            <span class="adm-field__label">Alt text (EN)</span>
            <input type="text" [(ngModel)]="altEn" maxlength="250" />
          </label>
          <label class="adm-field">
            <span class="adm-field__label">წარწერა</span>
            <input type="text" [(ngModel)]="captionKa" maxlength="500" />
          </label>
          <label class="adm-field">
            <span class="adm-field__label">ავტორი / წყარო</span>
            <input type="text" [(ngModel)]="creditKa" maxlength="200" />
          </label>
          <div class="adm-field adm-field--end">
            <button type="button" class="adm-btn adm-btn--primary" [disabled]="!file() || uploading() || (needsAlt() && !altKa.trim())" (click)="upload()">
              {{ uploading() ? 'იტვირთება…' : 'ატვირთვა და არჩევა' }}
            </button>
          </div>
        </div>
        @if (uploadError()) {
          <p class="adm-error-text">{{ uploadError() }}</p>
        }
        <p class="adm-hint">სურათები: JPG, PNG, WebP, GIF, SVG — მაქს. 10 MB. დოკუმენტები: PDF, PPTX, DOCX, XLSX — მაქს. 50 MB.</p>
      </section>

      <div class="adm-toolbar">
        <input type="search" placeholder="ძებნა ფაილის სახელით ან ტექსტით…" [(ngModel)]="search" (keyup.enter)="reload()" />
        <button type="button" class="adm-btn" (click)="reload()">ძებნა</button>
      </div>

      <div class="adm-media-grid">
        @for (media of items(); track media.id) {
          <button type="button" class="adm-media-tile" (click)="pick(media)" [title]="media.originalFileName">
            @if (media.kind === 'Image') {
              <img [src]="media.url" [alt]="media.altText.ka ?? ''" loading="lazy" />
            } @else {
              <span class="adm-media-tile__doc">{{ extension(media.originalFileName) }}</span>
            }
            <span class="adm-media-tile__name">{{ media.originalFileName }}</span>
            <span class="adm-media-tile__meta">{{ size(media.sizeBytes) }}@if (media.missingAltText) { · <b class="adm-warn">alt არ აქვს</b>}</span>
          </button>
        } @empty {
          @if (!loading()) {
            <p class="adm-empty">ფაილები ვერ მოიძებნა.</p>
          }
        }
      </div>
      @if (hasMore()) {
        <div class="adm-center"><button type="button" class="adm-btn" [disabled]="loading()" (click)="more()">მეტის ჩვენება</button></div>
      }
    </dialog>
  `,
  changeDetection: ChangeDetectionStrategy.Default,
})
export class MediaPicker {
  private readonly api = inject(AdminApi);
  private readonly dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');

  /** `Image` limits the library and uploads to images. */
  readonly kind = input<'Image' | null>(null);
  readonly picked = output<MediaRef>();
  readonly closed = output<void>();

  protected readonly items = signal<MediaAdmin[]>([]);
  protected readonly loading = signal(false);
  protected readonly hasMore = signal(false);
  protected readonly file = signal<File | null>(null);
  protected readonly uploading = signal(false);
  protected readonly uploadError = signal<string | null>(null);
  protected search = '';
  protected altKa = '';
  protected altEn = '';
  protected captionKa = '';
  protected creditKa = '';
  private page = 1;

  constructor() {
    afterNextRender(() => {
      this.dialog().nativeElement.showModal();
      this.reload();
    });
  }

  protected imagesOnly(): boolean {
    return this.kind() === 'Image';
  }

  protected accept(): string {
    return this.imagesOnly() ? 'image/*,.svg' : '';
  }

  protected needsAlt(): boolean {
    const file = this.file();
    return this.imagesOnly() || (file !== null && /\.(png|jpe?g|gif|webp|svg)$/i.test(file.name));
  }

  protected reload(): void {
    this.page = 1;
    this.items.set([]);
    this.fetch();
  }

  protected more(): void {
    this.page++;
    this.fetch();
  }

  protected onFile(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.file.set(input.files?.[0] ?? null);
    this.uploadError.set(null);
  }

  protected upload(): void {
    const file = this.file();
    if (!file) {
      return;
    }
    this.uploading.set(true);
    this.uploadError.set(null);
    this.api.upload(file, { altKa: this.altKa, altEn: this.altEn, captionKa: this.captionKa, creditKa: this.creditKa }).subscribe({
      next: (media) => {
        this.uploading.set(false);
        this.pick(media);
      },
      error: (error: unknown) => {
        this.uploading.set(false);
        const problem = toProblem(error);
        this.uploadError.set([problem.title, problem.detail, ...problem.fields.flatMap((f) => f.messages)].filter(Boolean).join(' '));
      },
    });
  }

  protected pick(media: MediaAdmin): void {
    this.picked.emit(toMediaRef(media));
    this.close();
  }

  protected close(): void {
    this.dialog().nativeElement.close();
  }

  protected size(bytes: number): string {
    return formatBytes(bytes);
  }

  protected extension(name: string): string {
    return name.split('.').pop()?.toUpperCase() ?? 'FILE';
  }

  private fetch(): void {
    this.loading.set(true);
    this.api.media.list({ kind: this.kind(), search: this.search, page: this.page, pageSize: 24 }).subscribe({
      next: (result) => {
        this.items.update((items) => [...items, ...result.items]);
        this.hasMore.set(result.hasMore);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}
