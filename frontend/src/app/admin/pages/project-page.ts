import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { AdmUrlPipe } from '../core/admin-project.service';
import { EMPTY, Observable } from 'rxjs';

import { LText, MediaRef, ProjectAdmin, SocialLink, lt } from '../core/admin.models';
import { formatUtc, orNull } from '../core/admin-time';
import { EditorPage } from '../core/editor-page';
import { LTextField } from '../ui/ltext-field';
import { MediaField } from '../ui/media-field';
import { Collapsible, ProblemBox } from '../ui/widgets';

interface PartnerRow {
  organizationId: string;
  role: string;
  roleLabel: LText;
  showInHeader: boolean;
}

interface ProjectForm {
  code: string;
  name: LText;
  shortName: LText;
  tagline: LText;
  status: string;
  startDate: string;
  endDate: string;
  fundingShortText: LText;
  fundingFullText: LText;
  logo: MediaRef | null;
  contactPerson: LText;
  contactEmail: string;
  contactPhone: string;
  contactAddress: LText;
  seoTitle: LText;
  seoDescription: LText;
  shareImage: MediaRef | null;
  partners: PartnerRow[];
  socialLinks: SocialLink[];
}

/** Admin-only project settings, including the funding and partnership identification (spec §2). */
@Component({
  selector: 'adm-project-page',
  imports: [FormsModule, RouterLink, LTextField, MediaField, Collapsible, ProblemBox, AdmUrlPipe],
  template: `
    <header class="adm-page-head">
      <div>
        <h1>პროექტი და დაფინანსება</h1>
        @if (item(); as p) {
          <p class="adm-meta">
            <span>{{ p.code }}</span>
            <span>ბოლოს შეცვალა {{ p.audit.updatedBy ?? '—' }}, {{ when(p.audit.updatedAtUtc) }}</span>
            <a [href]="'/projects/' + p.slug" target="_blank" rel="noopener">საიტზე ნახვა ↗</a>
          </p>
        }
      </div>
    </header>
    <adm-problem [problem]="problem()" />

    @if (form(); as f) {
      <section class="adm-card">
        <h2 class="adm-card__title">დაფინანსებისა და პარტნიორობის იდენტიფიკაცია</h2>
        <p class="adm-hint">ჩანს ყველა საჯარო გვერდზე: მოკლე ფორმულირება — Header-ის ქვემოთ ზოლში, სრული — Footer-ში.</p>
        <adm-ltext label="მოკლე ფორმულირება (Header)" kind="textarea" [rows]="2" [value]="f.fundingShortText" [required]="true" [maxLength]="300" />
        <adm-ltext label="სრული ფორმულირება (Footer)" kind="textarea" [value]="f.fundingFullText" [required]="true" [maxLength]="1000" />

        <h3 class="adm-card__title" style="margin-top: 8px">დამფინანსებელი და პარტნიორები</h3>
        <p class="adm-hint">
          ფონდი — „დამფინანსებელი“, BTU და ATSU — „პარტნიორი უნივერსიტეტი“. ისინი საიტზე ცალ-ცალკე ჩანს და არასოდეს ერევა ერთმანეთში.
          ლოგოები იმართება <a [routerLink]="'organizations' | admUrl">ორგანიზაციებში</a>.
        </p>
        <div class="adm-rows">
          @for (r of f.partners; track $index; let i = $index, first = $first, last = $last) {
            <div class="adm-row">
              <div class="adm-row__line">
                <label class="adm-row__label">ორგანიზაცია
                  <select [(ngModel)]="r.organizationId">
                    <option value="" disabled>— აირჩიეთ —</option>
                    @for (o of lookups.value().organizations; track o.id) {
                      <option [value]="o.id">{{ o.label }}</option>
                    }
                  </select>
                </label>
                <label class="adm-row__label">როლი
                  <select [(ngModel)]="r.role">
                    @for (o of lookups.value().partnerRoles; track o.value) {
                      <option [value]="o.value">{{ o.label }}</option>
                    }
                  </select>
                </label>
                <label class="adm-check"><input type="checkbox" [(ngModel)]="r.showInHeader" /><span>Header-ის ზოლში</span></label>
                <button type="button" class="adm-icon-btn" [disabled]="first" aria-label="ზემოთ" (click)="move(f.partners, i, -1)">↑</button>
                <button type="button" class="adm-icon-btn" [disabled]="last" aria-label="ქვემოთ" (click)="move(f.partners, i, 1)">↓</button>
                <button type="button" class="adm-icon-btn adm-icon-btn--danger" aria-label="მოხსნა" (click)="f.partners.splice(i, 1)">✕</button>
              </div>
              <adm-ltext label="როლის წარწერა (არასავალდ.)" [value]="r.roleLabel" [maxLength]="100" />
            </div>
          }
        </div>
        <button type="button" class="adm-btn adm-btn--sm" (click)="f.partners.push({ organizationId: '', role: 'PartnerUniversity', roleLabel: { ka: '', en: '' }, showInHeader: true })">
          + ორგანიზაციის დამატება
        </button>
      </section>

      <section class="adm-card">
        <h2 class="adm-card__title">პროექტი</h2>
        <adm-ltext label="დასახელება" [value]="f.name" [required]="true" [maxLength]="200" />
        <div class="adm-grid adm-grid--2">
          <adm-ltext label="მოკლე დასახელება" [value]="f.shortName" [maxLength]="80" />
          <adm-ltext label="სლოგანი" [value]="f.tagline" [maxLength]="200" />
        </div>
        <div class="adm-grid adm-grid--3">
          <label class="adm-field"><span class="adm-field__label">შიფრი</span><input type="text" [(ngModel)]="f.code" /></label>
          <label class="adm-field"><span class="adm-field__label">დაწყება</span><input type="date" [(ngModel)]="f.startDate" /></label>
          <label class="adm-field"><span class="adm-field__label">დასრულება</span><input type="date" [(ngModel)]="f.endDate" /></label>
        </div>
        <label class="adm-field">
          <span class="adm-field__label">სტატუსი</span>
          <select [(ngModel)]="f.status">
            <option value="Upcoming">დაგეგმილი</option>
            <option value="Active">მიმდინარე</option>
            <option value="Completed">დასრულებული</option>
          </select>
        </label>
        <adm-media-field label="პროექტის ლოგო" [(media)]="f.logo" />
      </section>

      <section class="adm-card">
        <h2 class="adm-card__title">კონტაქტი და სოციალური ქსელები</h2>
        <div class="adm-grid adm-grid--2">
          <adm-ltext label="საკონტაქტო პირი" [value]="f.contactPerson" [maxLength]="150" />
          <adm-ltext label="მისამართი" [value]="f.contactAddress" [maxLength]="300" />
          <label class="adm-field"><span class="adm-field__label">ელფოსტა</span><input type="email" [(ngModel)]="f.contactEmail" /></label>
          <label class="adm-field"><span class="adm-field__label">ტელეფონი</span><input type="text" [(ngModel)]="f.contactPhone" /></label>
        </div>
        <div class="adm-rows">
          @for (s of f.socialLinks; track $index; let i = $index) {
            <div class="adm-row__line">
              <select [(ngModel)]="s.platform" style="flex: 0 0 160px" aria-label="ქსელი">
                @for (o of lookups.value().socialPlatforms; track o.value) {
                  <option [value]="o.value">{{ o.label }}</option>
                }
              </select>
              <input type="url" [(ngModel)]="s.url" placeholder="https://" aria-label="ბმული" />
              <button type="button" class="adm-icon-btn adm-icon-btn--danger" aria-label="მოხსნა" (click)="f.socialLinks.splice(i, 1)">✕</button>
            </div>
          }
        </div>
        <button type="button" class="adm-btn adm-btn--sm" (click)="f.socialLinks.push({ platform: 'Facebook', url: '' })">+ ბმულის დამატება</button>
      </section>

      <adm-collapsible title="ნაგულისხმევი SEO">
        <adm-ltext label="SEO სათაური" [value]="f.seoTitle" [maxLength]="70" />
        <adm-ltext label="SEO აღწერა" kind="textarea" [value]="f.seoDescription" [maxLength]="200" />
        <adm-media-field label="გაზიარების სურათი" [(media)]="f.shareImage" />
      </adm-collapsible>

      <div class="adm-savebar">
        <span class="adm-hint">{{ dirty() ? 'არის შეუნახავი ცვლილებები' : 'ყველა ცვლილება შენახულია' }}</span>
        <button type="button" class="adm-btn adm-btn--primary" [disabled]="busy() || !dirty()" (click)="save()">{{ busy() ? 'ინახება…' : 'შენახვა' }}</button>
      </div>
    } @else if (loading()) {
      <p class="adm-empty">იტვირთება…</p>
    }
  `,
  changeDetection: ChangeDetectionStrategy.Default,
})
export class ProjectPage extends EditorPage<ProjectAdmin, ProjectForm> {
  protected readonly listPath = 'project';

  protected fetch() {
    return this.api.projectSettings();
  }

  protected createItem(): Observable<ProjectAdmin> {
    return EMPTY;
  }

  protected updateItem(_id: string, request: object) {
    return this.api.updateProjectSettings(request);
  }

  protected toForm(p: ProjectAdmin | null): ProjectForm {
    return {
      code: p?.code ?? '',
      name: lt(p?.name),
      shortName: lt(p?.shortName),
      tagline: lt(p?.tagline),
      status: p?.status ?? 'Active',
      startDate: p?.startDate ?? '',
      endDate: p?.endDate ?? '',
      fundingShortText: lt(p?.fundingShortText),
      fundingFullText: lt(p?.fundingFullText),
      logo: p?.logo ?? null,
      contactPerson: lt(p?.contactPerson),
      contactEmail: p?.contactEmail ?? '',
      contactPhone: p?.contactPhone ?? '',
      contactAddress: lt(p?.contactAddress),
      seoTitle: lt(p?.seoTitle),
      seoDescription: lt(p?.seoDescription),
      shareImage: p?.shareImage ?? null,
      partners: [...(p?.partners ?? [])]
        .sort((a, b) => a.displayOrder - b.displayOrder)
        .map((x) => ({ organizationId: x.organizationId, role: x.role, roleLabel: lt(x.roleLabel), showInHeader: x.showInHeader })),
      socialLinks: (p?.socialLinks ?? []).map((s) => ({ ...s })),
    };
  }

  protected toRequest(f: ProjectForm): object {
    return {
      code: f.code,
      name: f.name,
      shortName: f.shortName,
      tagline: f.tagline,
      status: f.status,
      startDate: f.startDate,
      endDate: f.endDate,
      fundingShortText: f.fundingShortText,
      fundingFullText: f.fundingFullText,
      logoId: f.logo?.id ?? null,
      contactPerson: f.contactPerson,
      contactEmail: orNull(f.contactEmail),
      contactPhone: orNull(f.contactPhone),
      contactAddress: f.contactAddress,
      seoTitle: f.seoTitle,
      seoDescription: f.seoDescription,
      shareImageId: f.shareImage?.id ?? null,
      partners: f.partners.filter((p) => p.organizationId),
      socialLinks: f.socialLinks.filter((s) => s.url.trim()),
    };
  }

  protected when(utc: string | null): string {
    return formatUtc(utc);
  }

  protected move(list: PartnerRow[], index: number, delta: number): void {
    const [row] = list.splice(index, 1);
    list.splice(index + delta, 0, row);
  }
}
