import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { AdmUrlPipe } from '../core/admin-project.service';

import { LText, MediaRef, ResourceAdmin, lt } from '../core/admin.models';
import { formatUtc, orNull, tbilisiInputToUtc, utcToTbilisiInput } from '../core/admin-time';
import { EditorPage } from '../core/editor-page';
import { LTextField } from '../ui/ltext-field';
import { MediaField } from '../ui/media-field';
import { CheckList, ProblemBox, StatusBadge } from '../ui/widgets';

interface ResourceForm {
  title: LText;
  slug: string;
  description: LText;
  type: string;
  source: 'file' | 'link';
  file: MediaRef | null;
  externalUrl: string;
  authorId: string | null;
  authorName: LText;
  eventId: string | null;
  thumbnail: MediaRef | null;
  status: string;
  publishedAt: string;
  isFeatured: boolean;
  tagIds: string[];
  disciplineIds: string[];
}

@Component({
  selector: 'adm-resource-editor',
  imports: [FormsModule, RouterLink, LTextField, MediaField, CheckList, ProblemBox, StatusBadge, AdmUrlPipe],
  template: `
    <header class="adm-page-head">
      <div>
        <a class="adm-back" [routerLink]="'resources' | admUrl">← რესურსები</a>
        <h1>{{ isNew() ? 'ახალი რესურსი' : (item()?.title?.ka ?? '…') }}</h1>
        @if (item(); as r) {
          <p class="adm-meta">
            <adm-status [value]="r.status" />
            <span>ბოლოს შეცვალა {{ r.audit.updatedBy ?? '—' }}, {{ when(r.audit.updatedAtUtc) }}</span>
            @if (r.status === 'Published') {
              <a [href]="r.publicPath" target="_blank" rel="noopener">საიტზე ნახვა ↗</a>
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
            <adm-ltext label="მოკლე აღწერა" kind="textarea" [value]="f.description" [maxLength]="1000" />
            <div class="adm-grid adm-grid--2">
              <label class="adm-field">
                <span class="adm-field__label">ტიპი</span>
                <select [(ngModel)]="f.type" (ngModelChange)="onTypeChange(f)">
                  @for (o of lookups.value().resourceTypes; track o.value) {
                    <option [value]="o.value">{{ o.label }}</option>
                  }
                </select>
              </label>
              <label class="adm-field">
                <span class="adm-field__label">წყარო</span>
                <select [(ngModel)]="f.source">
                  <option value="file" [disabled]="f.type === 'Video'">ატვირთული ფაილი</option>
                  <option value="link">გარე ბმული</option>
                </select>
              </label>
            </div>
            @if (f.source === 'file') {
              <adm-media-field label="ფაილი" [kind]="null" [(media)]="f.file" hint="PDF, PPTX, DOCX, XLSX ან სურათი. დიდი ვიდეოები — მხოლოდ ბმულით." />
            } @else {
              <label class="adm-field">
                <span class="adm-field__label">ბმული <span class="adm-req">*</span></span>
                <input type="url" [(ngModel)]="f.externalUrl" placeholder="https://www.youtube.com/watch?v=… ან https://…" />
                <span class="adm-hint">ვიდეო ინახება YouTube/Vimeo-ზე; საიტზე ჩაშენდება.</span>
              </label>
            }
            <adm-media-field label="მინიატურა (არასავალდებულო)" [(media)]="f.thumbnail" />
          </section>

          <section class="adm-card">
            <h2 class="adm-card__title">ავტორი და ღონისძიება</h2>
            <label class="adm-field">
              <span class="adm-field__label">ავტორი / მომხსენებელი (პროფილი)</span>
              <select [(ngModel)]="f.authorId">
                <option [ngValue]="null">— არცერთი —</option>
                @for (p of lookups.value().people; track p.id) {
                  <option [ngValue]="p.id">{{ p.label }}</option>
                }
              </select>
            </label>
            @if (!f.authorId) {
              <adm-ltext label="ან ავტორის სახელი ტექსტით" [value]="f.authorName" [maxLength]="150" />
            }
            <label class="adm-field">
              <span class="adm-field__label">დაკავშირებული ღონისძიება</span>
              <select [(ngModel)]="f.eventId">
                <option [ngValue]="null">— არცერთი —</option>
                @for (e of lookups.value().events; track e.id) {
                  <option [ngValue]="e.id">{{ e.label }}</option>
                }
              </select>
              <span class="adm-hint">მასალა გამოჩნდება ღონისძიების არქივის გვერდზე.</span>
            </label>
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
              <span class="adm-field__label">გამოქვეყნების თარიღი (თბილისი)</span>
              <input type="datetime-local" [(ngModel)]="f.publishedAt" />
              <span class="adm-hint">ცარიელი — გამოქვეყნების მომენტი.</span>
            </label>
            <label class="adm-check">
              <input type="checkbox" [(ngModel)]="f.isFeatured" />
              <span>რჩეული (მთავარ გვერდზე)</span>
            </label>
            <adm-check-list label="მიმართულებები" [options]="lookups.value().disciplines" [(selected)]="f.disciplineIds" />
            <adm-check-list label="თეგები" [options]="lookups.value().tags" [(selected)]="f.tagIds" />
            <label class="adm-field">
              <span class="adm-field__label">Slug (URL)</span>
              <input type="text" [(ngModel)]="f.slug" placeholder="ავტომატურად სათაურიდან" />
            </label>
            @if (item()?.status === 'Archived') {
              <button type="button" class="adm-btn adm-btn--danger adm-btn--block" [disabled]="busy()" (click)="remove()">რესურსის წაშლა</button>
            } @else if (item()) {
              <p class="adm-hint">წასაშლელად ჯერ დააყენეთ „დაარქივებული“ და შეინახეთ.</p>
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
export class ResourceEditorPage extends EditorPage<ResourceAdmin, ResourceForm> {
  protected readonly listPath = 'resources';

  protected fetch(id: string) {
    return this.api.resources.get(id);
  }

  protected createItem(request: object) {
    return this.api.resources.create(request);
  }

  protected updateItem(id: string, request: object) {
    return this.api.resources.update(id, request);
  }

  protected toForm(r: ResourceAdmin | null): ResourceForm {
    return {
      title: lt(r?.title),
      slug: r?.slug ?? '',
      description: lt(r?.description),
      type: r?.type ?? 'Presentation',
      source: r && !r.file && r.externalUrl ? 'link' : 'file',
      file: r?.file ?? null,
      externalUrl: r?.externalUrl ?? '',
      authorId: r?.authorId ?? null,
      authorName: lt(r?.authorName),
      eventId: r?.eventId ?? null,
      thumbnail: r?.thumbnail ?? null,
      status: r?.status ?? 'Published',
      publishedAt: utcToTbilisiInput(r?.publishedAtUtc),
      isFeatured: r?.isFeatured ?? false,
      tagIds: [...(r?.tagIds ?? [])],
      disciplineIds: [...(r?.disciplineIds ?? [])],
    };
  }

  protected toRequest(f: ResourceForm): object {
    return {
      title: f.title,
      slug: orNull(f.slug),
      description: f.description,
      type: f.type,
      fileId: f.source === 'file' ? (f.file?.id ?? null) : null,
      externalUrl: f.source === 'link' ? orNull(f.externalUrl) : null,
      authorId: f.authorId,
      authorName: f.authorId ? null : f.authorName,
      eventId: f.eventId,
      thumbnailId: f.thumbnail?.id ?? null,
      status: f.status,
      publishedAtUtc: tbilisiInputToUtc(f.publishedAt),
      isFeatured: f.isFeatured,
      tagIds: f.tagIds,
      disciplineIds: f.disciplineIds,
    };
  }

  /** Videos are never uploaded (spec §9): they are linked from YouTube/Vimeo. */
  protected onTypeChange(f: ResourceForm): void {
    if (f.type === 'Video') {
      f.source = 'link';
    }
  }

  protected when(utc: string | null): string {
    return formatUtc(utc);
  }

  protected remove(): void {
    this.deleteItem((id) => this.api.resources.remove(id), 'წავშალოთ ეს რესურსი? ატვირთული ფაილი მედიაში დარჩება.');
  }
}
