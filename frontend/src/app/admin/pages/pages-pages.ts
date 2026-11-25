import { ChangeDetectionStrategy, Component, effect, inject, signal, untracked } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { AdmUrlPipe } from '../core/admin-project.service';
import { EMPTY, Observable } from 'rxjs';

import { AdminApi } from '../core/admin-api.service';
import { AdminProject } from '../core/admin-project.service';
import { LText, MediaRef, PageAdmin, PageListItem, lt } from '../core/admin.models';
import { formatUtc, orNull } from '../core/admin-time';
import { EditorPage } from '../core/editor-page';
import { ProblemView, toProblem } from '../core/problem';
import { LTextField } from '../ui/ltext-field';
import { MediaField } from '../ui/media-field';
import { Collapsible, ProblemBox } from '../ui/widgets';

const pageNames: Readonly<Record<string, string>> = {
  home: 'მთავარი გვერდი (Hero)',
  about: 'პროექტის შესახებ',
  news: 'სიახლეები',
  'stem-saturdays': 'STEM შაბათობები',
  team: 'ჩვენი გუნდი',
  researchers: 'მკვლევრები',
  mentors: 'მენტორები',
  'stem-ambassadors': 'STEM ელჩები (პროგრამის გვერდი)',
  resources: 'რესურსები',
  contact: 'კონტაქტი',
};

@Component({
  selector: 'adm-pages-list',
  imports: [RouterLink, ProblemBox, AdmUrlPipe],
  template: `
    <header class="adm-page-head"><h1>გვერდები</h1></header>
    <p class="adm-lead">
      საიტის ძირითადი გვერდების სათაური, შესავალი ტექსტი, მენიუში გამოჩენა და SEO. მთავარი გვერდის „Hero“ ბლოკი იმართება „მთავარი გვერდის“ ჩანაწერით.
    </p>
    <adm-problem [problem]="problem()" />
    <div class="adm-table-wrap">
      <table class="adm-table">
        <thead>
          <tr><th>გვერდი</th><th>სათაური საიტზე</th><th>მენიუში</th><th>ბოლო ცვლილება</th><th></th></tr>
        </thead>
        <tbody>
          @for (p of pages(); track p.key) {
            <tr>
              <td><a class="adm-table__title" [routerLink]="['pages' | admUrl, p.key]">{{ name(p.key) }}</a></td>
              <td>{{ p.title }}</td>
              <td>{{ p.showInNavigation ? '✓ (' + p.navigationOrder + ')' : '—' }}</td>
              <td>{{ when(p.updatedAtUtc) }}<small class="adm-hint">{{ p.updatedBy }}</small></td>
              <td><a [href]="p.publicPath" target="_blank" rel="noopener">ნახვა ↗</a></td>
            </tr>
          }
        </tbody>
      </table>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PagesListPage {
  private readonly api = inject(AdminApi);
  private readonly project = inject(AdminProject);
  protected readonly pages = signal<PageListItem[]>([]);
  protected readonly problem = signal<ProblemView | null>(null);

  constructor() {
    effect(() => {
      this.project.slug();
      untracked(() => this.api.pages().subscribe({ next: (p) => this.pages.set(p), error: (e: unknown) => this.problem.set(toProblem(e)) }));
    });
  }

  protected name(key: string): string {
    return pageNames[key] ?? key;
  }

  protected when(utc: string): string {
    return formatUtc(utc);
  }
}

interface PageForm {
  title: LText;
  navigationLabel: LText;
  lead: LText;
  body: LText;
  heroImage: MediaRef | null;
  primaryActionLabel: LText;
  primaryActionUrl: string;
  secondaryActionLabel: LText;
  secondaryActionUrl: string;
  showInNavigation: boolean;
  navigationOrder: number;
  seoTitle: LText;
  seoDescription: LText;
  shareImage: MediaRef | null;
}

@Component({
  selector: 'adm-page-editor',
  imports: [FormsModule, RouterLink, LTextField, MediaField, Collapsible, ProblemBox, AdmUrlPipe],
  template: `
    <header class="adm-page-head">
      <div>
        <a class="adm-back" [routerLink]="'pages' | admUrl">← გვერდები</a>
        <h1>{{ name() }}</h1>
        @if (item(); as p) {
          <p class="adm-meta">
            <span>ბოლოს შეცვალა {{ p.audit.updatedBy ?? '—' }}, {{ when(p.audit.updatedAtUtc) }}</span>
            <a [href]="p.publicPath" target="_blank" rel="noopener">საიტზე ნახვა ↗</a>
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
            <adm-ltext label="შესავალი (lead)" kind="textarea" [value]="f.lead" [maxLength]="1000" />
            <adm-ltext label="ძირითადი ტექსტი" kind="rich" [value]="f.body" />
            <adm-media-field label="სურათი (Hero)" [(media)]="f.heroImage" />
          </section>

          <section class="adm-card">
            <h2 class="adm-card__title">ღილაკები (არასავალდებულო)</h2>
            <div class="adm-grid adm-grid--2">
              <div>
                <adm-ltext label="მთავარი ღილაკის ტექსტი" [value]="f.primaryActionLabel" [maxLength]="60" />
                <label class="adm-field">
                  <span class="adm-field__label">მთავარი ღილაკის ბმული</span>
                  <input type="text" [(ngModel)]="f.primaryActionUrl" placeholder="/projects/school2university/stem-saturdays ან https://…" />
                </label>
              </div>
              <div>
                <adm-ltext label="მეორე ღილაკის ტექსტი" [value]="f.secondaryActionLabel" [maxLength]="60" />
                <label class="adm-field">
                  <span class="adm-field__label">მეორე ღილაკის ბმული</span>
                  <input type="text" [(ngModel)]="f.secondaryActionUrl" />
                </label>
              </div>
            </div>
          </section>

          <adm-collapsible title="SEO და გაზიარება">
            <adm-ltext label="SEO სათაური" [value]="f.seoTitle" [maxLength]="70" />
            <adm-ltext label="SEO აღწერა" kind="textarea" [value]="f.seoDescription" [maxLength]="200" />
            <adm-media-field label="გაზიარების სურათი" [(media)]="f.shareImage" />
          </adm-collapsible>
        </div>

        <aside class="adm-editor__side">
          <section class="adm-card">
            <h2 class="adm-card__title">მენიუ</h2>
            <label class="adm-check">
              <input type="checkbox" [(ngModel)]="f.showInNavigation" />
              <span>მთავარ მენიუში ჩვენება</span>
            </label>
            <adm-ltext label="მენიუს წარწერა" [value]="f.navigationLabel" [maxLength]="40" hint="ცარიელი — გამოიყენება სათაური." />
            <label class="adm-field">
              <span class="adm-field__label">რიგი მენიუში</span>
              <input type="number" [(ngModel)]="f.navigationOrder" />
            </label>
          </section>
        </aside>
      </div>

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
export class PageEditorPage extends EditorPage<PageAdmin, PageForm> {
  protected readonly listPath = 'pages';

  protected name(): string {
    return pageNames[this.id()] ?? this.id();
  }

  protected fetch(key: string) {
    return this.api.page(key);
  }

  /** Pages are fixed; they are never created from the CMS. */
  protected createItem(): Observable<PageAdmin> {
    return EMPTY;
  }

  protected updateItem(_id: string, request: object) {
    return this.api.updatePage(this.id(), request);
  }

  protected toForm(p: PageAdmin | null): PageForm {
    return {
      title: lt(p?.title),
      navigationLabel: lt(p?.navigationLabel),
      lead: lt(p?.lead),
      body: lt(p?.body),
      heroImage: p?.heroImage ?? null,
      primaryActionLabel: lt(p?.primaryActionLabel),
      primaryActionUrl: p?.primaryActionUrl ?? '',
      secondaryActionLabel: lt(p?.secondaryActionLabel),
      secondaryActionUrl: p?.secondaryActionUrl ?? '',
      showInNavigation: p?.showInNavigation ?? false,
      navigationOrder: p?.navigationOrder ?? 0,
      seoTitle: lt(p?.seoTitle),
      seoDescription: lt(p?.seoDescription),
      shareImage: p?.shareImage ?? null,
    };
  }

  protected toRequest(f: PageForm): object {
    return {
      title: f.title,
      navigationLabel: f.navigationLabel,
      lead: f.lead,
      body: f.body,
      heroImageId: f.heroImage?.id ?? null,
      primaryActionLabel: f.primaryActionLabel,
      primaryActionUrl: orNull(f.primaryActionUrl),
      secondaryActionLabel: f.secondaryActionLabel,
      secondaryActionUrl: orNull(f.secondaryActionUrl),
      showInNavigation: f.showInNavigation,
      navigationOrder: f.navigationOrder ?? 0,
      seoTitle: f.seoTitle,
      seoDescription: f.seoDescription,
      shareImageId: f.shareImage?.id ?? null,
    };
  }

  protected when(utc: string | null): string {
    return formatUtc(utc);
  }
}
