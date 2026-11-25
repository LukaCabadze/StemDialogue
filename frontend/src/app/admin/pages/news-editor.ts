import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { AdmUrlPipe } from '../core/admin-project.service';

import { LText, MediaRef, NewsAdmin, lt } from '../core/admin.models';
import { formatUtc, orNull, parseUtc, tbilisiInputToUtc } from '../core/admin-time';
import { EditorPage } from '../core/editor-page';
import { LTextField } from '../ui/ltext-field';
import { GalleryField, MediaField } from '../ui/media-field';
import { CheckList, Collapsible, ProblemBox, StatusBadge } from '../ui/widgets';

interface NewsForm {
  title: LText;
  slug: string;
  summary: LText;
  body: LText;
  category: string;
  coverImage: MediaRef | null;
  gallery: MediaRef[];
  tagIds: string[];
  relatedEventId: string | null;
  isFeatured: boolean;
  seoTitle: LText;
  seoDescription: LText;
  shareImage: MediaRef | null;
}

@Component({
  selector: 'adm-news-editor',
  imports: [FormsModule, RouterLink, LTextField, MediaField, GalleryField, CheckList, Collapsible, ProblemBox, StatusBadge, AdmUrlPipe],
  template: `
    <header class="adm-page-head">
      <div>
        <a class="adm-back" [routerLink]="'news' | admUrl">← სიახლეები</a>
        <h1>{{ isNew() ? 'ახალი სიახლე' : (item()?.title?.ka ?? '…') }}</h1>
        @if (item(); as n) {
          <p class="adm-meta">
            <adm-status [value]="n.status" />
            @if (isScheduled(n)) {
              <adm-status value="Scheduled" [label]="'გამოქვეყნდება ' + when(n.publishAtUtc)" />
            } @else if (n.publishAtUtc) {
              <span>გამოქვეყნდა {{ when(n.publishAtUtc) }}</span>
            }
            <span>ბოლოს შეცვალა {{ n.audit.updatedBy ?? '—' }}, {{ when(n.audit.updatedAtUtc) }}</span>
            @if (n.status === 'Published') {
              <a [href]="n.publicPath" target="_blank" rel="noopener">საიტზე ნახვა ↗</a>
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
            <adm-ltext label="ანოტაცია (მოკლე აღწერა)" kind="textarea" [value]="f.summary" [required]="true" [maxLength]="500"
              hint="ჩანს ბარათზე და საძიებო სისტემებში. 1–2 წინადადება." />
            <adm-ltext label="სრული ტექსტი" kind="rich" [value]="f.body" [required]="true" hint="საჭიროა გამოქვეყნებისთვის." />
          </section>

          <section class="adm-card">
            <adm-media-field label="მთავარი სურათი" [(media)]="f.coverImage" hint="რეკომენდებული: ჰორიზონტალური, მინ. 1200px სიგანე." />
            <adm-gallery-field label="ფოტო გალერეა (არასავალდებულო)" [(items)]="f.gallery" />
          </section>

          <adm-collapsible title="SEO და გაზიარება (არასავალდებულო)">
            <adm-ltext label="SEO სათაური" [value]="f.seoTitle" [maxLength]="70" hint="ცარიელის შემთხვევაში გამოიყენება სათაური." />
            <adm-ltext label="SEO აღწერა" kind="textarea" [value]="f.seoDescription" [maxLength]="200" hint="ცარიელის შემთხვევაში გამოიყენება ანოტაცია." />
            <adm-media-field label="გაზიარების სურათი" [(media)]="f.shareImage" hint="ცარიელის შემთხვევაში გამოიყენება მთავარი სურათი." />
          </adm-collapsible>
        </div>

        <aside class="adm-editor__side">
          <section class="adm-card">
            <h2 class="adm-card__title">გამოქვეყნება</h2>
            @if (item(); as n) {
              @if (n.status !== 'Published') {
                <label class="adm-field">
                  <span class="adm-field__label">გამოქვეყნების დრო (თბილისი)</span>
                  <input type="datetime-local" [(ngModel)]="publishAt" />
                  <span class="adm-hint">ცარიელი — ახლავე. მომავალი თარიღი — დაგეგმილი გამოქვეყნება.</span>
                </label>
                <button type="button" class="adm-btn adm-btn--primary adm-btn--block" [disabled]="busy()" (click)="publish()">
                  {{ publishAt ? 'გამოქვეყნების დაგეგმვა' : 'გამოქვეყნება' }}
                </button>
              } @else {
                <button type="button" class="adm-btn adm-btn--block" [disabled]="busy()" (click)="unpublish()">დრაფტად დაბრუნება</button>
              }
              @if (n.status !== 'Archived') {
                <button type="button" class="adm-btn adm-btn--block" [disabled]="busy()" (click)="archive()">დაარქივება</button>
              }
              @if (!n.firstPublishedAtUtc) {
                <button type="button" class="adm-btn adm-btn--danger adm-btn--block" [disabled]="busy()" (click)="remove()">დრაფტის წაშლა</button>
              }
            } @else {
              <p class="adm-hint">ჯერ შეინახეთ დრაფტად, შემდეგ გამოაქვეყნეთ.</p>
            }
          </section>

          <section class="adm-card">
            <label class="adm-field">
              <span class="adm-field__label">კატეგორია <span class="adm-req">*</span></span>
              <select [(ngModel)]="f.category">
                @for (c of lookups.value().newsCategories; track c.value) {
                  <option [value]="c.value">{{ c.label }}</option>
                }
              </select>
            </label>
            <label class="adm-check">
              <input type="checkbox" [(ngModel)]="f.isFeatured" />
              <span>მთავარ გვერდზე გამორჩეული</span>
            </label>
            <label class="adm-field">
              <span class="adm-field__label">დაკავშირებული შეხვედრა</span>
              <select [(ngModel)]="f.relatedEventId">
                <option [ngValue]="null">— არცერთი —</option>
                @for (e of lookups.value().events; track e.id) {
                  <option [ngValue]="e.id">{{ e.label }}</option>
                }
              </select>
            </label>
            <adm-check-list label="თეგები" [options]="lookups.value().tags" [(selected)]="f.tagIds" empty="თეგები დაამატეთ „თეგები და მიმართულებები“ გვერდზე." />
            <label class="adm-field">
              <span class="adm-field__label">Slug (URL)</span>
              <input type="text" [(ngModel)]="f.slug" placeholder="ავტომატურად სათაურიდან" />
              <span class="adm-hint">გამოქვეყნებულის შეცვლისას ძველი ბმული ავტომატურად გადამისამართდება.</span>
            </label>
          </section>
        </aside>
      </div>

      <div class="adm-savebar">
        <span class="adm-hint">{{ dirty() ? 'არის შეუნახავი ცვლილებები' : 'ყველა ცვლილება შენახულია' }}</span>
        <button type="button" class="adm-btn adm-btn--primary" [disabled]="busy() || (!dirty() && !isNew())" (click)="save()">
          {{ busy() ? 'ინახება…' : isNew() ? 'დრაფტად შენახვა' : 'შენახვა' }}
        </button>
      </div>
    } @else if (loading()) {
      <p class="adm-empty">იტვირთება…</p>
    }
  `,
  changeDetection: ChangeDetectionStrategy.Default,
})
export class NewsEditorPage extends EditorPage<NewsAdmin, NewsForm> {
  protected readonly listPath = 'news';
  protected publishAt = '';

  protected fetch(id: string) {
    return this.api.news.get(id);
  }

  protected createItem(request: object) {
    return this.api.news.create(request);
  }

  protected updateItem(id: string, request: object) {
    return this.api.news.update(id, request);
  }

  protected toForm(n: NewsAdmin | null): NewsForm {
    return {
      title: lt(n?.title),
      slug: n?.slug ?? '',
      summary: lt(n?.summary),
      body: lt(n?.body),
      category: n?.category ?? 'Announcement',
      coverImage: n?.coverImage ?? null,
      gallery: [...(n?.gallery ?? [])],
      tagIds: [...(n?.tagIds ?? [])],
      relatedEventId: n?.relatedEventId ?? null,
      isFeatured: n?.isFeatured ?? false,
      seoTitle: lt(n?.seoTitle),
      seoDescription: lt(n?.seoDescription),
      shareImage: n?.shareImage ?? null,
    };
  }

  protected toRequest(f: NewsForm): object {
    return {
      title: f.title,
      slug: orNull(f.slug),
      summary: f.summary,
      body: f.body,
      category: f.category,
      coverImageId: f.coverImage?.id ?? null,
      galleryImageIds: f.gallery.map((m) => m.id),
      tagIds: f.tagIds,
      relatedEventId: f.relatedEventId,
      isFeatured: f.isFeatured,
      seoTitle: f.seoTitle,
      seoDescription: f.seoDescription,
      shareImageId: f.shareImage?.id ?? null,
    };
  }

  protected override afterSave(): void {
    this.publishAt = '';
  }

  protected isScheduled(n: NewsAdmin): boolean {
    return n.status === 'Published' && !!n.publishAtUtc && parseUtc(n.publishAtUtc) > new Date();
  }

  protected when(utc: string | null): string {
    return formatUtc(utc);
  }

  protected publish(): void {
    const publishAtUtc = tbilisiInputToUtc(this.publishAt);
    this.runAction((n) => this.api.news.action(n.id, 'publish', { publishAtUtc }), publishAtUtc ? 'გამოქვეყნება დაიგეგმა' : 'გამოქვეყნდა');
  }

  protected unpublish(): void {
    this.runAction((n) => this.api.news.action(n.id, 'unpublish'), 'დაბრუნდა დრაფტად');
  }

  protected archive(): void {
    if (confirm('სიახლე დაარქივდეს? საჯარო სიიდან გაქრება.')) {
      this.runAction((n) => this.api.news.action(n.id, 'archive'), 'დაარქივდა');
    }
  }

  protected remove(): void {
    this.deleteItem((id) => this.api.news.remove(id), 'წავშალოთ ეს დრაფტი? მოქმედება შეუქცევადია.');
  }
}
