import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { AdmUrlPipe } from '../core/admin-project.service';

import { LText, MediaRef, PersonAdmin, lt } from '../core/admin.models';
import { formatUtc, orNull } from '../core/admin-time';
import { EditorPage } from '../core/editor-page';
import { LTextField } from '../ui/ltext-field';
import { MediaField } from '../ui/media-field';
import { CheckList, Collapsible, ProblemBox, StatusBadge } from '../ui/widgets';

interface RoleRow {
  projectId: string;
  role: string;
  title: LText;
  startDate: string;
  endDate: string;
  displayOrder: number;
}

interface LinkRow {
  type: string;
  url: string;
  label: LText;
}

interface PersonForm {
  fullName: LText;
  slug: string;
  academicTitle: LText;
  position: LText;
  organization: LText;
  shortBio: LText;
  biography: LText;
  contribution: LText;
  photo: MediaRef | null;
  email: string;
  status: string;
  displayOrder: number;
  roles: RoleRow[];
  links: LinkRow[];
  disciplineIds: string[];
  seoTitle: LText;
  seoDescription: LText;
  shareImage: MediaRef | null;
}

@Component({
  selector: 'adm-person-editor',
  imports: [FormsModule, RouterLink, LTextField, MediaField, CheckList, Collapsible, ProblemBox, StatusBadge, AdmUrlPipe],
  template: `
    <header class="adm-page-head">
      <div>
        <a class="adm-back" [routerLink]="'people' | admUrl">← ადამიანები</a>
        <h1>{{ isNew() ? 'ახალი პროფილი' : (item()?.fullName?.ka ?? '…') }}</h1>
        @if (item(); as p) {
          <p class="adm-meta">
            <adm-status [value]="p.status" />
            <span>ბოლოს შეცვალა {{ p.audit.updatedBy ?? '—' }}, {{ when(p.audit.updatedAtUtc) }}</span>
            @if (p.status === 'Published') {
              @for (path of p.publicPaths; track path) {
                <a [href]="path" target="_blank" rel="noopener">{{ path.split('/').slice(-2, -1)[0] }} ↗</a>
              }
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
            <adm-ltext label="სახელი, გვარი" [value]="f.fullName" [required]="true" [maxLength]="150" />
            <div class="adm-grid adm-grid--2">
              <adm-ltext label="აკადემიური წოდება" [value]="f.academicTitle" [maxLength]="100" hint="მაგ.: პროფესორი, PhD" />
              <adm-ltext label="პოზიცია" [value]="f.position" [maxLength]="150" />
            </div>
            <adm-ltext label="ორგანიზაცია" [value]="f.organization" [maxLength]="200" />
            <adm-ltext label="მოკლე ბიოგრაფია" kind="textarea" [value]="f.shortBio" [maxLength]="500" hint="ჩანს ბარათზე, 1–3 წინადადება." />
            <adm-ltext label="სრული ბიოგრაფია" kind="rich" [value]="f.biography" />
            <adm-ltext label="წვლილი პროექტში" kind="textarea" [value]="f.contribution" [maxLength]="1000" hint="რას აკეთებს/გააკეთა პროექტში." />
          </section>

          <section class="adm-card">
            <h2 class="adm-card__title">როლები პროექტში</h2>
            <p class="adm-hint">
              გუნდის სექცია, მკვლევარი, მენტორი… ერთ ადამიანს შეიძლება ჰქონდეს რამდენიმე როლი. წასულ წევრს მიუთითეთ დასრულების თარიღი —
              ის დარჩება „ყოფილი წევრების“ არქივში.
            </p>
            <div class="adm-rows">
              @for (r of f.roles; track $index; let i = $index) {
                <div class="adm-row">
                  <div class="adm-row__line">
                    <label class="adm-row__label">როლი
                      <select [(ngModel)]="r.role">
                        @for (o of lookups.value().personRoles; track o.value) {
                          <option [value]="o.value">{{ o.label }}</option>
                        }
                      </select>
                    </label>
                    @if (lookups.value().projects.length > 1) {
                      <label class="adm-row__label">პროექტი
                        <select [(ngModel)]="r.projectId">
                          @for (o of lookups.value().projects; track o.id) {
                            <option [value]="o.id">{{ o.label }}</option>
                          }
                        </select>
                      </label>
                    }
                    <label class="adm-row__label">დაწყება<input type="date" [(ngModel)]="r.startDate" /></label>
                    <label class="adm-row__label">დასრულება<input type="date" [(ngModel)]="r.endDate" /></label>
                    <label class="adm-row__label" style="flex: 0 0 90px">რიგი<input type="number" [(ngModel)]="r.displayOrder" /></label>
                    <button type="button" class="adm-icon-btn adm-icon-btn--danger" aria-label="როლის წაშლა" (click)="f.roles.splice(i, 1)">✕</button>
                  </div>
                  <adm-ltext label="თანამდებობა ამ როლში" [value]="r.title" [maxLength]="150" hint="მაგ.: პროექტის ხელმძღვანელი, კოორდინატორი" />
                </div>
              }
            </div>
            <button type="button" class="adm-btn adm-btn--sm" (click)="addRole(f)">+ როლის დამატება</button>
          </section>

          <section class="adm-card">
            <h2 class="adm-card__title">ბმულები</h2>
            <div class="adm-rows">
              @for (l of f.links; track $index; let i = $index) {
                <div class="adm-row">
                  <div class="adm-row__line">
                    <select [(ngModel)]="l.type" aria-label="ტიპი" style="flex: 0 0 170px">
                      @for (o of lookups.value().personLinkTypes; track o.value) {
                        <option [value]="o.value">{{ o.label }}</option>
                      }
                    </select>
                    <input type="url" [(ngModel)]="l.url" placeholder="https://" aria-label="მისამართი" />
                    <input type="text" [(ngModel)]="l.label.ka" placeholder="წარწერა (არასავალდ.)" aria-label="წარწერა" />
                    <button type="button" class="adm-icon-btn adm-icon-btn--danger" aria-label="ბმულის წაშლა" (click)="f.links.splice(i, 1)">✕</button>
                  </div>
                </div>
              }
            </div>
            <button type="button" class="adm-btn adm-btn--sm" (click)="f.links.push({ type: 'Website', url: '', label: { ka: '', en: '' } })">+ ბმულის დამატება</button>
          </section>

          <adm-collapsible title="SEO და გაზიარება (არასავალდებულო)">
            <adm-ltext label="SEO სათაური" [value]="f.seoTitle" [maxLength]="70" />
            <adm-ltext label="SEO აღწერა" kind="textarea" [value]="f.seoDescription" [maxLength]="200" />
            <adm-media-field label="გაზიარების სურათი" [(media)]="f.shareImage" />
          </adm-collapsible>
        </div>

        <aside class="adm-editor__side">
          <section class="adm-card">
            <label class="adm-field">
              <span class="adm-field__label">ხილვადობა</span>
              <select [(ngModel)]="f.status">
                @for (s of lookups.value().publicationStatuses; track s.value) {
                  <option [value]="s.value">{{ s.label }}</option>
                }
              </select>
              <span class="adm-hint">„დრაფტი“ — საიტზე არ ჩანს. „დაარქივებული“ — დამალულია, მაგრამ ისტორია ინახება.</span>
            </label>
            <adm-media-field label="ფოტო" [(media)]="f.photo" hint="კვადრატული ან პორტრეტული, მინ. 600px." />
            <label class="adm-field">
              <span class="adm-field__label">რიგითობა</span>
              <input type="number" [(ngModel)]="f.displayOrder" />
              <span class="adm-hint">უფრო მცირე რიცხვი — უფრო წინ სიაში.</span>
            </label>
            <label class="adm-field">
              <span class="adm-field__label">ელფოსტა (არ ქვეყნდება)</span>
              <input type="email" [(ngModel)]="f.email" />
            </label>
            <adm-check-list label="მიმართულებები" [options]="lookups.value().disciplines" [(selected)]="f.disciplineIds" />
            <label class="adm-field">
              <span class="adm-field__label">Slug (URL)</span>
              <input type="text" [(ngModel)]="f.slug" placeholder="ავტომატურად სახელიდან" />
            </label>
            @if (item(); as p) {
              @if (p.status === 'Archived') {
                <button type="button" class="adm-btn adm-btn--danger adm-btn--block" [disabled]="busy()" (click)="remove()">პროფილის წაშლა</button>
                <p class="adm-hint">წაიშლება მხოლოდ თუ ღონისძიებებს/რესურსებს არ უკავშირდება.</p>
              } @else {
                <p class="adm-hint">წასაშლელად ჯერ დააყენეთ „დაარქივებული“ და შეინახეთ. წასულ წევრს უმჯობესია როლის დასრულების თარიღი მიუთითოთ.</p>
              }
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
export class PersonEditorPage extends EditorPage<PersonAdmin, PersonForm> {
  protected readonly listPath = 'people';

  protected fetch(id: string) {
    return this.api.people.get(id);
  }

  protected createItem(request: object) {
    return this.api.people.create(request);
  }

  protected updateItem(id: string, request: object) {
    return this.api.people.update(id, request);
  }

  protected toForm(p: PersonAdmin | null): PersonForm {
    return {
      fullName: lt(p?.fullName),
      slug: p?.slug ?? '',
      academicTitle: lt(p?.academicTitle),
      position: lt(p?.position),
      organization: lt(p?.organization),
      shortBio: lt(p?.shortBio),
      biography: lt(p?.biography),
      contribution: lt(p?.contribution),
      photo: p?.photo ?? null,
      email: p?.email ?? '',
      status: p?.status ?? 'Published',
      displayOrder: p?.displayOrder ?? 100,
      roles: (p?.roles ?? []).map((r) => ({
        projectId: r.projectId,
        role: r.role,
        title: lt(r.title),
        startDate: r.startDate ?? '',
        endDate: r.endDate ?? '',
        displayOrder: r.displayOrder,
      })),
      links: (p?.links ?? []).map((l) => ({ type: l.type, url: l.url, label: lt(l.label) })),
      disciplineIds: [...(p?.disciplineIds ?? [])],
      seoTitle: lt(p?.seoTitle),
      seoDescription: lt(p?.seoDescription),
      shareImage: p?.shareImage ?? null,
    };
  }

  protected toRequest(f: PersonForm): object {
    return {
      fullName: f.fullName,
      slug: orNull(f.slug),
      academicTitle: f.academicTitle,
      position: f.position,
      organization: f.organization,
      shortBio: f.shortBio,
      biography: f.biography,
      contribution: f.contribution,
      photoId: f.photo?.id ?? null,
      email: orNull(f.email),
      status: f.status,
      displayOrder: f.displayOrder ?? 0,
      roles: f.roles.map((r) => ({
        projectId: r.projectId,
        role: r.role,
        title: r.title,
        startDate: orNull(r.startDate),
        endDate: orNull(r.endDate),
        displayOrder: r.displayOrder ?? 0,
      })),
      links: f.links.filter((l) => l.url.trim()).map((l) => ({ type: l.type, url: l.url.trim(), label: l.label })),
      disciplineIds: f.disciplineIds,
      seoTitle: f.seoTitle,
      seoDescription: f.seoDescription,
      shareImageId: f.shareImage?.id ?? null,
    };
  }

  protected override afterSave(): void {
    this.lookups.reload();
  }

  protected when(utc: string | null): string {
    return formatUtc(utc);
  }

  protected addRole(f: PersonForm): void {
    f.roles.push({
      projectId: this.lookups.value().projects[0]?.id ?? '',
      role: 'Researcher',
      title: lt(null),
      startDate: new Date().toISOString().slice(0, 10),
      endDate: '',
      displayOrder: 100,
    });
  }

  protected remove(): void {
    this.deleteItem((id) => this.api.people.remove(id), 'წავშალოთ ეს პროფილი? მოქმედება შეუქცევადია.');
  }
}
