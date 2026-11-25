import { ChangeDetectionStrategy, Component, input, model, signal } from '@angular/core';

import { MediaRef } from '../core/admin.models';
import { MediaPicker } from './media-picker';

/** One image or file chosen from the media library. */
@Component({
  selector: 'adm-media-field',
  imports: [MediaPicker],
  template: `
    <div class="adm-field">
      <span class="adm-field__label">{{ label() }}</span>
      <div class="adm-media-field">
        @if (media(); as m) {
          @if (m.kind === 'Image') {
            <img class="adm-media-field__thumb" [src]="m.url" [alt]="m.altKa ?? ''" />
          } @else {
            <span class="adm-media-field__doc">{{ m.originalFileName.split('.').pop()?.toUpperCase() }}</span>
          }
          <div class="adm-media-field__info">
            <a [href]="m.url" target="_blank" rel="noopener">{{ m.originalFileName }}</a>
            @if (m.kind === 'Image' && !m.altKa) {
              <span class="adm-warn">Alt ტექსტი არ აქვს — დაამატეთ „მედიაში“</span>
            }
          </div>
          <button type="button" class="adm-btn adm-btn--sm" (click)="open.set(true)">შეცვლა</button>
          <button type="button" class="adm-btn adm-btn--sm adm-btn--ghost" (click)="media.set(null)">მოხსნა</button>
        } @else {
          <button type="button" class="adm-btn adm-btn--sm" (click)="open.set(true)">{{ kind() === 'Image' ? 'სურათის არჩევა' : 'ფაილის არჩევა' }}</button>
        }
      </div>
      @if (hint()) {
        <p class="adm-hint">{{ hint() }}</p>
      }
    </div>
    @if (open()) {
      <adm-media-picker [kind]="kind()" (picked)="media.set($event)" (closed)="open.set(false)" />
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MediaField {
  readonly label = input.required<string>();
  readonly kind = input<'Image' | null>('Image');
  readonly hint = input<string | null>(null);
  readonly media = model<MediaRef | null>(null);
  protected readonly open = signal(false);
}

/** Ordered list of images (galleries). */
@Component({
  selector: 'adm-gallery-field',
  imports: [MediaPicker],
  template: `
    <div class="adm-field">
      <span class="adm-field__label">{{ label() }}</span>
      <div class="adm-gallery">
        @for (m of items(); track m.id; let i = $index, first = $first, last = $last) {
          <figure class="adm-gallery__item">
            <img [src]="m.url" [alt]="m.altKa ?? ''" />
            <figcaption>
              <button type="button" class="adm-icon-btn" [disabled]="first" aria-label="წინ გადატანა" (click)="move(i, -1)">←</button>
              <button type="button" class="adm-icon-btn" [disabled]="last" aria-label="უკან გადატანა" (click)="move(i, 1)">→</button>
              <button type="button" class="adm-icon-btn adm-icon-btn--danger" aria-label="მოხსნა" (click)="remove(i)">✕</button>
            </figcaption>
            @if (!m.altKa) {
              <span class="adm-gallery__warn" title="Alt ტექსტი არ აქვს">!</span>
            }
          </figure>
        }
        <button type="button" class="adm-gallery__add" (click)="open.set(true)">+ ფოტოს დამატება</button>
      </div>
      @if (hint()) {
        <p class="adm-hint">{{ hint() }}</p>
      }
    </div>
    @if (open()) {
      <adm-media-picker kind="Image" (picked)="add($event)" (closed)="open.set(false)" />
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GalleryField {
  readonly label = input.required<string>();
  readonly hint = input<string | null>(null);
  readonly items = model<MediaRef[]>([]);
  protected readonly open = signal(false);

  protected add(media: MediaRef): void {
    if (!this.items().some((m) => m.id === media.id)) {
      this.items.set([...this.items(), media]);
    }
  }

  protected remove(index: number): void {
    this.items.set(this.items().filter((_, i) => i !== index));
  }

  protected move(index: number, delta: number): void {
    const next = [...this.items()];
    const [item] = next.splice(index, 1);
    next.splice(index + delta, 0, item);
    this.items.set(next);
  }
}
