import { ChangeDetectionStrategy, Component, input, linkedSignal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { AdmUrlPipe } from '../core/admin-project.service';

import { ActivityListItem, EventListItem, NewsListItem, PersonListItem, ResourceListItem } from '../core/admin.models';
import { ListPage } from '../core/list-page';
import { Pager, ProblemBox, StatusBadge } from '../ui/widgets';

const listImports = [FormsModule, RouterLink, Pager, ProblemBox, StatusBadge, AdmUrlPipe];

@Component({
  selector: 'adm-news-list',
  imports: listImports,
  template: `
    <header class="adm-page-head">
      <h1>სიახლეები</h1>
      <div class="adm-page-head__actions"><a class="adm-btn adm-btn--primary" [routerLink]="'news/new' | admUrl">+ ახალი სიახლე</a></div>
    </header>
    <div class="adm-toolbar">
      <input type="search" placeholder="ძებნა სათაურით…" [(ngModel)]="search" (keyup.enter)="reload()" />
      <select [(ngModel)]="status" (ngModelChange)="reload()" aria-label="სტატუსი">
        <option value="">ყველა სტატუსი</option>
        @for (s of lookups.value().publicationStatuses; track s.value) {
          <option [value]="s.value">{{ s.label }}</option>
        }
      </select>
      <select [(ngModel)]="category" (ngModelChange)="reload()" aria-label="კატეგორია">
        <option value="">ყველა კატეგორია</option>
        @for (c of lookups.value().newsCategories; track c.value) {
          <option [value]="c.value">{{ c.label }}</option>
        }
      </select>
      <button type="button" class="adm-btn" (click)="reload()">ძებნა</button>
    </div>
    <adm-problem [problem]="problem()" />
    <div class="adm-table-wrap">
      <table class="adm-table">
        <thead>
          <tr><th>სათაური</th><th>კატეგორია</th><th>სტატუსი</th><th>გამოქვეყნება</th><th>ბოლო ცვლილება</th></tr>
        </thead>
        <tbody>
          @for (n of result()?.items; track n.id) {
            <tr>
              <td>
                <a class="adm-table__title" [routerLink]="['news' | admUrl, n.id]">{{ n.title }}</a>
                @if (n.isFeatured) {<span class="adm-tag">მთავარზე</span>}
              </td>
              <td>{{ lookups.label(n.category) }}</td>
              <td>
                @if (n.isScheduled) {
                  <adm-status value="Scheduled" label="დაგეგმილი" />
                } @else {
                  <adm-status [value]="n.status" />
                }
              </td>
              <td>{{ when(n.publishAtUtc) }}</td>
              <td>{{ when(n.updatedAtUtc) }}<br /><small class="adm-hint">{{ n.updatedBy }}</small></td>
            </tr>
          } @empty {
            <tr><td colspan="5" class="adm-empty">{{ loading() ? 'იტვირთება…' : 'სიახლეები ვერ მოიძებნა.' }}</td></tr>
          }
        </tbody>
      </table>
    </div>
    @if (result(); as r) {
      <adm-pager [page]="r.page" [totalPages]="r.totalPages" [total]="r.total" (pageChange)="goTo($event)" />
    }
  `,
  changeDetection: ChangeDetectionStrategy.Default,
})
export class NewsListPage extends ListPage<NewsListItem> {
  readonly statusParam = input<string | undefined>(undefined, { alias: 'status' });
  protected readonly status = linkedSignal(() => this.statusParam() ?? '');
  protected category = '';
  protected search = '';

  protected fetch(page: number) {
    return this.api.news.list({ page, pageSize: 20, status: this.status(), category: this.category, search: this.search });
  }
}

@Component({
  selector: 'adm-events-list',
  imports: listImports,
  template: `
    <header class="adm-page-head">
      <h1>STEM შაბათობები და ღონისძიებები</h1>
      <div class="adm-page-head__actions"><a class="adm-btn adm-btn--primary" [routerLink]="'events/new' | admUrl">+ ახალი ღონისძიება</a></div>
    </header>
    <div class="adm-toolbar">
      <input type="search" placeholder="ძებნა სათაურით…" [(ngModel)]="search" (keyup.enter)="reload()" />
      <select [(ngModel)]="scope" (ngModelChange)="reload()" aria-label="დრო">
        <option value="All">ყველა</option>
        <option value="Upcoming">მომავალი</option>
        <option value="Past">წარსული / არქივი</option>
      </select>
      <select [(ngModel)]="status" (ngModelChange)="reload()" aria-label="სტატუსი">
        <option value="">ყველა სტატუსი</option>
        @for (s of lookups.value().eventStatuses; track s.value) {
          <option [value]="s.value">{{ s.label }}</option>
        }
      </select>
      <button type="button" class="adm-btn" (click)="reload()">ძებნა</button>
    </div>
    <adm-problem [problem]="problem()" />
    <div class="adm-table-wrap">
      <table class="adm-table">
        <thead>
          <tr><th>სათაური</th><th>თარიღი (თბილისი)</th><th>სტატუსი</th><th>საჯაროობა</th><th>მონაწილე</th></tr>
        </thead>
        <tbody>
          @for (e of result()?.items; track e.id) {
            <tr [class.adm-row-warn]="e.awaitingCompletion">
              <td>
                <a class="adm-table__title" [routerLink]="['events' | admUrl, e.id]">{{ e.title }}</a>
                <small class="adm-hint">{{ lookups.label(e.type) }}</small>
                @if (e.awaitingCompletion) {<span class="adm-tag adm-tag--warn">ჩატარდა? მონიშნეთ</span>}
                @if (e.isFeatured) {<span class="adm-tag">მთავარზე</span>}
              </td>
              <td>{{ when(e.startsAtUtc) }}</td>
              <td><adm-status [value]="e.status" /></td>
              <td><adm-status [value]="e.publication" /></td>
              <td>{{ e.participantCount }}</td>
            </tr>
          } @empty {
            <tr><td colspan="5" class="adm-empty">{{ loading() ? 'იტვირთება…' : 'ღონისძიებები ვერ მოიძებნა.' }}</td></tr>
          }
        </tbody>
      </table>
    </div>
    @if (result(); as r) {
      <adm-pager [page]="r.page" [totalPages]="r.totalPages" [total]="r.total" (pageChange)="goTo($event)" />
    }
  `,
  changeDetection: ChangeDetectionStrategy.Default,
})
export class EventsListPage extends ListPage<EventListItem> {
  readonly scopeParam = input<string | undefined>(undefined, { alias: 'scope' });
  protected readonly scope = linkedSignal(() => this.scopeParam() ?? 'All');
  protected status = '';
  protected search = '';

  protected fetch(page: number) {
    return this.api.events.list({ page, pageSize: 20, scope: this.scope(), status: this.status, search: this.search });
  }
}

@Component({
  selector: 'adm-people-list',
  imports: listImports,
  template: `
    <header class="adm-page-head">
      <h1>ადამიანები</h1>
      <div class="adm-page-head__actions"><a class="adm-btn adm-btn--primary" [routerLink]="'people/new' | admUrl">+ ახალი პროფილი</a></div>
    </header>
    <p class="adm-lead">ერთი ადამიანი = ერთი პროფილი. გუნდის წევრობა, მკვლევრობა და მენტორობა არის როლები ამ პროფილზე.</p>
    <div class="adm-toolbar">
      <input type="search" placeholder="ძებნა სახელით…" [(ngModel)]="search" (keyup.enter)="reload()" />
      <select [(ngModel)]="role" (ngModelChange)="reload()" aria-label="როლი">
        <option value="">ყველა როლი</option>
        @for (r of lookups.value().personRoles; track r.value) {
          <option [value]="r.value">{{ r.label }}</option>
        }
      </select>
      <select [(ngModel)]="status" (ngModelChange)="reload()" aria-label="სტატუსი">
        <option value="">ყველა სტატუსი</option>
        @for (s of lookups.value().publicationStatuses; track s.value) {
          <option [value]="s.value">{{ s.label }}</option>
        }
      </select>
      <button type="button" class="adm-btn" (click)="reload()">ძებნა</button>
    </div>
    <adm-problem [problem]="problem()" />
    <div class="adm-table-wrap">
      <table class="adm-table">
        <thead>
          <tr><th>სახელი</th><th>ორგანიზაცია</th><th>როლები</th><th>სტატუსი</th><th>ღონისძ.</th></tr>
        </thead>
        <tbody>
          @for (p of result()?.items; track p.id) {
            <tr>
              <td>
                <a class="adm-table__title" [routerLink]="['people' | admUrl, p.id]">{{ p.fullName }}</a>
                @if (!p.hasPhoto) {<small class="adm-hint">ფოტოს გარეშე</small>}
              </td>
              <td>{{ p.organization ?? '—' }}</td>
              <td>
                @for (r of p.roles; track $index) {
                  <span class="adm-tag">{{ lookups.label(r) }}</span>
                }
              </td>
              <td><adm-status [value]="p.status" /></td>
              <td>{{ p.eventCount }}</td>
            </tr>
          } @empty {
            <tr><td colspan="5" class="adm-empty">{{ loading() ? 'იტვირთება…' : 'პროფილები ვერ მოიძებნა.' }}</td></tr>
          }
        </tbody>
      </table>
    </div>
    @if (result(); as r) {
      <adm-pager [page]="r.page" [totalPages]="r.totalPages" [total]="r.total" (pageChange)="goTo($event)" />
    }
  `,
  changeDetection: ChangeDetectionStrategy.Default,
})
export class PeopleListPage extends ListPage<PersonListItem> {
  readonly statusParam = input<string | undefined>(undefined, { alias: 'status' });
  protected readonly status = linkedSignal(() => this.statusParam() ?? '');
  protected role = '';
  protected search = '';

  protected fetch(page: number) {
    return this.api.people.list({ page, pageSize: 25, role: this.role, status: this.status(), search: this.search });
  }
}

@Component({
  selector: 'adm-resources-list',
  imports: listImports,
  template: `
    <header class="adm-page-head">
      <h1>სასწავლო რესურსები</h1>
      <div class="adm-page-head__actions"><a class="adm-btn adm-btn--primary" [routerLink]="'resources/new' | admUrl">+ ახალი რესურსი</a></div>
    </header>
    <div class="adm-toolbar">
      <input type="search" placeholder="ძებნა…" [(ngModel)]="search" (keyup.enter)="reload()" />
      <select [(ngModel)]="type" (ngModelChange)="reload()" aria-label="ტიპი">
        <option value="">ყველა ტიპი</option>
        @for (t of lookups.value().resourceTypes; track t.value) {
          <option [value]="t.value">{{ t.label }}</option>
        }
      </select>
      <select [(ngModel)]="eventId" (ngModelChange)="reload()" aria-label="ღონისძიება">
        <option value="">ყველა ღონისძიება</option>
        @for (e of lookups.value().events; track e.id) {
          <option [value]="e.id">{{ e.label }}</option>
        }
      </select>
      <select [(ngModel)]="status" (ngModelChange)="reload()" aria-label="სტატუსი">
        <option value="">ყველა სტატუსი</option>
        @for (s of lookups.value().publicationStatuses; track s.value) {
          <option [value]="s.value">{{ s.label }}</option>
        }
      </select>
    </div>
    <adm-problem [problem]="problem()" />
    <div class="adm-table-wrap">
      <table class="adm-table">
        <thead>
          <tr><th>სათაური</th><th>ტიპი</th><th>ღონისძიება</th><th>სტატუსი</th><th>გამოქვეყნდა</th></tr>
        </thead>
        <tbody>
          @for (r of result()?.items; track r.id) {
            <tr>
              <td>
                <a class="adm-table__title" [routerLink]="['resources' | admUrl, r.id]">{{ r.title }}</a>
                @if (r.isFeatured) {<span class="adm-tag">რჩეული</span>}
              </td>
              <td>{{ lookups.label(r.type) }}</td>
              <td>{{ r.eventTitle ?? '—' }}</td>
              <td><adm-status [value]="r.status" /></td>
              <td>{{ when(r.publishedAtUtc) }}</td>
            </tr>
          } @empty {
            <tr><td colspan="5" class="adm-empty">{{ loading() ? 'იტვირთება…' : 'რესურსები ვერ მოიძებნა.' }}</td></tr>
          }
        </tbody>
      </table>
    </div>
    @if (result(); as r) {
      <adm-pager [page]="r.page" [totalPages]="r.totalPages" [total]="r.total" (pageChange)="goTo($event)" />
    }
  `,
  changeDetection: ChangeDetectionStrategy.Default,
})
export class ResourcesListPage extends ListPage<ResourceListItem> {
  protected type = '';
  protected eventId = '';
  protected status = '';
  protected search = '';

  protected fetch(page: number) {
    return this.api.resources.list({ page, pageSize: 20, type: this.type, eventId: this.eventId, status: this.status, search: this.search });
  }
}

@Component({
  selector: 'adm-activities-list',
  imports: listImports,
  template: `
    <header class="adm-page-head">
      <h1>STEM ელჩები — სასკოლო აქტივობები</h1>
      <div class="adm-page-head__actions">
        <a class="adm-btn" [routerLink]="'pages/stem-ambassadors' | admUrl">პროგრამის გვერდი</a>
        <a class="adm-btn adm-btn--primary" [routerLink]="'activities/new' | admUrl">+ ახალი აქტივობა</a>
      </div>
    </header>
    <p class="adm-lead">მოსწავლეების ფოტოები საჯაროდ ჩანს მხოლოდ მას შემდეგ, რაც მონიშნავთ, რომ თანხმობა მიღებულია. მოსწავლეთა პროფილები არ იქმნება.</p>
    <div class="adm-toolbar">
      <input type="search" placeholder="ძებნა…" [(ngModel)]="search" (keyup.enter)="reload()" />
      <select [(ngModel)]="status" (ngModelChange)="reload()" aria-label="სტატუსი">
        <option value="">ყველა სტატუსი</option>
        @for (s of lookups.value().publicationStatuses; track s.value) {
          <option [value]="s.value">{{ s.label }}</option>
        }
      </select>
    </div>
    <adm-problem [problem]="problem()" />
    <div class="adm-table-wrap">
      <table class="adm-table">
        <thead>
          <tr><th>სათაური</th><th>სკოლა</th><th>თარიღი</th><th>სტატუსი</th><th>თანხმობა</th></tr>
        </thead>
        <tbody>
          @for (a of result()?.items; track a.id) {
            <tr>
              <td><a class="adm-table__title" [routerLink]="['activities' | admUrl, a.id]">{{ a.title }}</a></td>
              <td>{{ a.schoolName ?? '—' }}</td>
              <td>{{ day(a.activityDate) }}</td>
              <td><adm-status [value]="a.status" /></td>
              <td>{{ a.mediaConsentConfirmed ? '✓ მიღებულია' : '—' }}</td>
            </tr>
          } @empty {
            <tr><td colspan="5" class="adm-empty">{{ loading() ? 'იტვირთება…' : 'აქტივობები ჯერ არ არის.' }}</td></tr>
          }
        </tbody>
      </table>
    </div>
    @if (result(); as r) {
      <adm-pager [page]="r.page" [totalPages]="r.totalPages" [total]="r.total" (pageChange)="goTo($event)" />
    }
  `,
  changeDetection: ChangeDetectionStrategy.Default,
})
export class ActivitiesListPage extends ListPage<ActivityListItem> {
  protected status = '';
  protected search = '';

  protected fetch(page: number) {
    return this.api.activities.list({ page, pageSize: 20, status: this.status, search: this.search });
  }
}
