import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { AdmUrlPipe } from '../core/admin-project.service';

import { ActivityAdmin, LText, MediaRef, lt } from '../core/admin.models';
import { formatUtc, orNull } from '../core/admin-time';
import { EditorPage } from '../core/editor-page';
import { LTextField } from '../ui/ltext-field';
import { GalleryField, MediaField } from '../ui/media-field';
import { ProblemBox, StatusBadge } from '../ui/widgets';

interface ActivityForm {
  title: LText;
  slug: string;
  summary: LText;
  body: LText;
  schoolName: LText;
  region: LText;
  activityDate: string;
  coverImage: MediaRef | null;
  gallery: MediaRef[];
  status: string;
  mediaConsentConfirmed: boolean;
  consentReference: string;
}

@Component({
  selector: 'adm-activity-editor',
  imports: [FormsModule, RouterLink, LTextField, MediaField, GalleryField, ProblemBox, StatusBadge, AdmUrlPipe],
  template: `
    <header class="adm-page-head">
      <div>
        <a class="adm-back" [routerLink]="'activities' | admUrl">← ელჩების აქტივობები</a>
        <h1>{{ isNew() ? 'ახალი სასკოლო აქტივობა' : (item()?.title?.ka ?? '…') }}</h1>
        @if (item(); as a) {
          <p class="adm-meta">
            <adm-status [value]="a.status" />
            <span>ბოლოს შეცვალა {{ a.audit.updatedBy ?? '—' }}, {{ when(a.audit.updatedAtUtc) }}</span>
            @if (a.status === 'Published') {
              <a [href]="a.publicPath" target="_blank" rel="noopener">საიტზე ნახვა ↗</a>
            }
          </p>
        }
      </div>
    </header>

    <adm-problem [problem]="problem()" />

    @if (form(); as f) {
      <div class="adm-editor">
        <div class="adm-editor__main">
          <section class="adm-card">
            <adm-ltext label="სათაური" [value]="f.title" [required]="true" [maxLength]="200" />
            <adm-ltext label="მოკლე აღწერა" kind="textarea" [value]="f.summary" [maxLength]="500" />
            <adm-ltext label="დეტალური აღწერა" kind="rich" [value]="f.body" />
            <div class="adm-grid adm-grid--2">
              <adm-ltext label="სკოლა" [value]="f.schoolName" [maxLength]="200" />
              <adm-ltext label="რეგიონი / ქალაქი" [value]="f.region" [maxLength]="100" />
            </div>
            <label class="adm-field">
              <span class="adm-field__label">თარიღი</span>
              <input type="date" [(ngModel)]="f.activityDate" />
            </label>
          </section>

          <section class="adm-card">
            <h2 class="adm-card__title">ფოტოები</h2>
            <div class="adm-alert adm-alert--warn">
              არასრულწლოვნების იდენტიფიცირებადი ფოტოები საჯაროდ ქვეყნდება მხოლოდ მშობლის/სკოლის წერილობითი თანხმობით.
            </div>
            <label class="adm-check">
              <input type="checkbox" [(ngModel)]="f.mediaConsentConfirmed" />
              <span>თანხმობა მიღებულია ფოტოების გამოქვეყნებაზე</span>
            </label>
            @if (f.mediaConsentConfirmed) {
              <label class="adm-field">
                <span class="adm-field__label">თანხმობის დოკუმენტის ნომერი / მითითება</span>
                <input type="text" [(ngModel)]="f.consentReference" maxlength="200" />
              </label>
            }
            <adm-media-field label="მთავარი სურათი" [(media)]="f.coverImage" />
            <adm-gallery-field label="გალერეა" [(items)]="f.gallery" />
          </section>
        </div>

        <aside class="adm-editor__side">
          <section class="adm-card">
            <label class="adm-field">
              <span class="adm-field__label">სტატუსი</span>
              <select [(ngModel)]="f.status">
                @for (s of lookups.value().publicationStatuses; track s.value) {
                  <option [value]="s.value">{{ s.label }}</option>
                }
              </select>
            </label>
            <label class="adm-field">
              <span class="adm-field__label">Slug (URL)</span>
              <input type="text" [(ngModel)]="f.slug" placeholder="ავტომატურად სათაურიდან" />
            </label>
            @if (item()) {
              <button type="button" class="adm-btn adm-btn--danger adm-btn--block" [disabled]="busy()" (click)="remove()">აქტივობის წაშლა</button>
            }
          </section>
        </aside>
      </div>

      <div class="adm-savebar">
        <span class="adm-hint">{{ dirty() ? 'არის შეუნახავი ცვლილებები' : 'ყველა ცვლილება შენახულია' }}</span>
        <button type="button" class="adm-btn adm-btn--primary" [disabled]="busy() || (!dirty() && !isNew())" (click)="save()">
          {{ busy() ? 'ინახება…' : 'შენახვა' }}
        </button>
      </div>
    } @else if (loading()) {
      <p class="adm-empty">იტვირთება…</p>
    }
  `,
  changeDetection: ChangeDetectionStrategy.Default,
})
export class ActivityEditorPage extends EditorPage<ActivityAdmin, ActivityForm> {
  protected readonly listPath = 'activities';

  protected fetch(id: string) {
    return this.api.activities.get(id);
  }

  protected createItem(request: object) {
    return this.api.activities.create(request);
  }

  protected updateItem(id: string, request: object) {
    return this.api.activities.update(id, request);
  }

  protected toForm(a: ActivityAdmin | null): ActivityForm {
    return {
      title: lt(a?.title),
      slug: a?.slug ?? '',
      summary: lt(a?.summary),
      body: lt(a?.body),
      schoolName: lt(a?.schoolName),
      region: lt(a?.region),
      activityDate: a?.activityDate ?? '',
      coverImage: a?.coverImage ?? null,
      gallery: [...(a?.gallery ?? [])],
      status: a?.status ?? 'Draft',
      mediaConsentConfirmed: a?.mediaConsentConfirmed ?? false,
      consentReference: a?.consentReference ?? '',
    };
  }

  protected toRequest(f: ActivityForm): object {
    return {
      title: f.title,
      slug: orNull(f.slug),
      summary: f.summary,
      body: f.body,
      schoolName: f.schoolName,
      region: f.region,
      activityDate: orNull(f.activityDate),
      coverImageId: f.coverImage?.id ?? null,
      galleryImageIds: f.gallery.map((m) => m.id),
      status: f.status,
      mediaConsentConfirmed: f.mediaConsentConfirmed,
      consentReference: f.mediaConsentConfirmed ? orNull(f.consentReference) : null,
    };
  }

  protected when(utc: string | null): string {
    return formatUtc(utc);
  }

  protected remove(): void {
    this.deleteItem((id) => this.api.activities.remove(id), 'წავშალოთ ეს აქტივობა?');
  }
}
