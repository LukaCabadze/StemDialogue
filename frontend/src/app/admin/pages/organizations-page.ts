import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { AdmUrlPipe } from '../core/admin-project.service';

import { AdminApi } from '../core/admin-api.service';
import { AdminAuth } from '../core/admin-auth.service';
import { LText, MediaRef, OrganizationAdmin, lt } from '../core/admin.models';
import { formatUtc, orNull } from '../core/admin-time';
import { EditorPage } from '../core/editor-page';
import { ProblemView, toProblem } from '../core/problem';
import { LTextField } from '../ui/ltext-field';
import { MediaField } from '../ui/media-field';
import { ProblemBox } from '../ui/widgets';

@Component({
  selector: 'adm-organizations-page',
  imports: [RouterLink, ProblemBox, AdmUrlPipe],
  template: `
    <header class="adm-page-head">
      <h1>ორგანიზაციები</h1>
      @if (auth.isAdmin()) {
        <div class="adm-page-head__actions"><a class="adm-btn adm-btn--primary" [routerLink]="'organizations/new' | admUrl">+ ახალი ორგანიზაცია</a></div>
      }
    </header>
    <p class="adm-lead">
      დამფინანსებელი და პარტნიორი უნივერსიტეტები ოფიციალური ლოგოებით. პროექტში მათი როლი (დამფინანსებელი / პარტნიორი) ეთითება
      „პროექტი და დაფინანსება“ გვერდზე.
    </p>
    <adm-problem [problem]="problem()" />
    <div class="adm-table-wrap">
      <table class="adm-table">
        <thead><tr><th>ლოგო</th><th>სახელი</th><th>ვებსაიტი</th><th>პროექტებში</th></tr></thead>
        <tbody>
          @for (o of items(); track o.id) {
            <tr>
              <td>
                @if (o.logo) {
                  <img [src]="o.logo.url" [alt]="o.logo.altKa ?? ''" style="height: 36px; width: auto" />
                }
              </td>
              <td>
                <a class="adm-table__title" [routerLink]="['organizations' | admUrl, o.id]">{{ o.name.ka }}</a>
                <small class="adm-hint">{{ o.shortName.ka }}</small>
              </td>
              <td>
                @if (o.websiteUrl) {
                  <a [href]="o.websiteUrl" target="_blank" rel="noopener">{{ o.websiteUrl }}</a>
                }
              </td>
              <td>{{ o.usedByProjects.join(', ') || '—' }}</td>
            </tr>
          } @empty {
            <tr><td colspan="4" class="adm-empty">ორგანიზაციები ჯერ არ არის.</td></tr>
          }
        </tbody>
      </table>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrganizationsPage {
  private readonly api = inject(AdminApi);
  protected readonly auth = inject(AdminAuth);
  protected readonly items = signal<OrganizationAdmin[]>([]);
  protected readonly problem = signal<ProblemView | null>(null);

  constructor() {
    this.api.organizations().subscribe({ next: (items) => this.items.set(items), error: (e: unknown) => this.problem.set(toProblem(e)) });
  }
}

interface OrganizationForm {
  name: LText;
  shortName: LText;
  description: LText;
  websiteUrl: string;
  logo: MediaRef | null;
  logoOnDark: MediaRef | null;
}

@Component({
  selector: 'adm-organization-editor',
  imports: [FormsModule, RouterLink, LTextField, MediaField, ProblemBox, AdmUrlPipe],
  template: `
    <header class="adm-page-head">
      <div>
        <a class="adm-back" [routerLink]="'organizations' | admUrl">← ორგანიზაციები</a>
        <h1>{{ isNew() ? 'ახალი ორგანიზაცია' : (item()?.name?.ka ?? '…') }}</h1>
        @if (item(); as o) {
          <p class="adm-meta"><span>ბოლოს შეცვალა {{ o.audit.updatedBy ?? '—' }}, {{ when(o.audit.updatedAtUtc) }}</span></p>
        }
      </div>
    </header>
    @if (!auth.isAdmin()) {
      <div class="adm-alert adm-alert--info">ორგანიზაციების შეცვლა შეუძლია მხოლოდ ადმინისტრატორს.</div>
    }
    <adm-problem [problem]="problem()" />

    @if (form(); as f) {
      <fieldset class="adm-card" [disabled]="!auth.isAdmin()" style="border: 1px solid var(--adm-line); margin: 0 0 16px">
        <adm-ltext label="ოფიციალური სახელი" [value]="f.name" [required]="true" [maxLength]="200" />
        <adm-ltext label="მოკლე სახელი / აბრევიატურა" [value]="f.shortName" [maxLength]="50" hint="მაგ.: ბტუ / BTU" />
        <adm-ltext label="აღწერა" kind="textarea" [value]="f.description" [maxLength]="1000" />
        <label class="adm-field">
          <span class="adm-field__label">ოფიციალური ვებსაიტი</span>
          <input type="url" [(ngModel)]="f.websiteUrl" placeholder="https://" />
        </label>
        <div class="adm-grid adm-grid--2">
          <adm-media-field label="ლოგო (ღია ფონზე)" [(media)]="f.logo" hint="ოფიციალური ფაილი, სასურველია SVG ან გამჭვირვალე PNG." />
          <adm-media-field label="ლოგო მუქ ფონზე (Footer)" [(media)]="f.logoOnDark" />
        </div>
      </fieldset>

      @if (auth.isAdmin()) {
        <div class="adm-savebar">
          @if (item(); as o) {
            @if (!o.usedByProjects.length) {
              <button type="button" class="adm-btn adm-btn--danger" [disabled]="busy()" (click)="remove()">წაშლა</button>
            }
          }
          <span class="adm-hint">{{ dirty() ? 'არის შეუნახავი ცვლილებები' : 'ყველა ცვლილება შენახულია' }}</span>
          <button type="button" class="adm-btn adm-btn--primary" [disabled]="busy() || (!dirty() && !isNew())" (click)="save()">შენახვა</button>
        </div>
      }
    }
  `,
  changeDetection: ChangeDetectionStrategy.Default,
})
export class OrganizationEditorPage extends EditorPage<OrganizationAdmin, OrganizationForm> {
  protected readonly listPath = 'organizations';

  protected fetch(id: string) {
    return this.api.organization(id);
  }

  protected createItem(request: object) {
    return this.api.createOrganization(request);
  }

  protected updateItem(id: string, request: object) {
    return this.api.updateOrganization(id, request);
  }

  protected toForm(o: OrganizationAdmin | null): OrganizationForm {
    return {
      name: lt(o?.name),
      shortName: lt(o?.shortName),
      description: lt(o?.description),
      websiteUrl: o?.websiteUrl ?? '',
      logo: o?.logo ?? null,
      logoOnDark: o?.logoOnDark ?? null,
    };
  }

  protected toRequest(f: OrganizationForm): object {
    return {
      name: f.name,
      shortName: f.shortName,
      description: f.description,
      websiteUrl: orNull(f.websiteUrl),
      logoId: f.logo?.id ?? null,
      logoOnDarkId: f.logoOnDark?.id ?? null,
    };
  }

  protected override afterSave(): void {
    this.lookups.reload();
  }

  override dirty(): boolean {
    return this.auth.isAdmin() && super.dirty();
  }

  protected when(utc: string | null): string {
    return formatUtc(utc);
  }

  protected remove(): void {
    this.deleteItem((id) => this.api.deleteOrganization(id), 'წავშალოთ ეს ორგანიზაცია?');
  }
}
