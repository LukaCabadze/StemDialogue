import { ChangeDetectionStrategy, Component, effect, inject, signal, untracked } from '@angular/core';
import { RouterLink } from '@angular/router';

import { AdmUrlPipe } from '../core/admin-project.service';

import { AdminApi } from '../core/admin-api.service';
import { AdminProject } from '../core/admin-project.service';
import { Dashboard } from '../core/admin.models';
import { formatBytes, formatUtc } from '../core/admin-time';
import { ProblemView, toProblem } from '../core/problem';
import { ProblemBox } from '../ui/widgets';

const actions: Readonly<Record<string, string>> = { Created: 'შეიქმნა', Updated: 'შეიცვალა', Deleted: 'წაიშალა' };

@Component({
  selector: 'adm-dashboard-page',
  imports: [RouterLink, ProblemBox, AdmUrlPipe],
  template: `
    <header class="adm-page-head">
      <h1>მთავარი პანელი</h1>
      <div class="adm-page-head__actions">
        <a class="adm-btn adm-btn--primary" [routerLink]="'news/new' | admUrl">+ სიახლე</a>
        <a class="adm-btn" [routerLink]="'events/new' | admUrl">+ STEM შაბათობა</a>
        <a class="adm-btn" [routerLink]="'people/new' | admUrl">+ ადამიანი</a>
      </div>
    </header>
    <adm-problem [problem]="problem()" />

    @if (data(); as d) {
      @if (d.eventsAwaitingCompletion > 0) {
        <div class="adm-alert adm-alert--warn">
          <strong>{{ d.eventsAwaitingCompletion }} ღონისძიება უკვე ჩატარდა, მაგრამ არ არის მონიშნული „ჩატარებულად“.</strong>
          <p>გახსენით და დაამატეთ შეჯამება, ფოტოები და ჩანაწერი — ასე გადავა არქივში იმავე URL-ზე.</p>
          <a [routerLink]="'events' | admUrl" [queryParams]="{ scope: 'Past' }">ღონისძიებების ნახვა →</a>
        </div>
      }
      @if (d.imagesWithoutAltText > 0) {
        <div class="adm-alert adm-alert--warn">
          <strong>{{ d.imagesWithoutAltText }} სურათს არ აქვს Alt ტექსტი.</strong>
          <a [routerLink]="'media' | admUrl" [queryParams]="{ kind: 'Image' }">მედიაში დამატება →</a>
        </div>
      }

      <div class="adm-stats">
        <a class="adm-stat" [routerLink]="'news' | admUrl" [queryParams]="{ status: 'Published' }"><b>{{ d.publishedNews }}</b><span>გამოქვეყნებული სიახლე</span></a>
        <a class="adm-stat" [routerLink]="'news' | admUrl" [queryParams]="{ status: 'Draft' }"><b>{{ d.draftNews }}</b><span>დრაფტი</span></a>
        <a class="adm-stat" [routerLink]="'news' | admUrl"><b>{{ d.scheduledNews }}</b><span>დაგეგმილი გამოქვეყნება</span></a>
        <a class="adm-stat" [routerLink]="'events' | admUrl" [queryParams]="{ scope: 'Upcoming' }"><b>{{ d.upcomingEvents }}</b><span>მომავალი ღონისძიება</span></a>
        <a class="adm-stat" [routerLink]="'people' | admUrl"><b>{{ d.publishedPeople }}</b><span>საჯარო პროფილი</span></a>
        <a class="adm-stat" [routerLink]="'people' | admUrl" [queryParams]="{ status: 'Draft' }"><b>{{ d.draftPeople }}</b><span>პროფილის დრაფტი</span></a>
        <a class="adm-stat" [routerLink]="'resources' | admUrl"><b>{{ d.publishedResources }}</b><span>რესურსი</span></a>
        <a class="adm-stat" [routerLink]="'media' | admUrl"><b>{{ bytes(d.mediaBytes) }}</b><span>მედიის მოცულობა</span></a>
      </div>

      <section class="adm-card">
        <h2 class="adm-card__title">ბოლო ცვლილებები</h2>
        <table class="adm-table">
          <thead>
            <tr><th>დრო</th><th>ვინ</th><th>რა</th><th>მოქმედება</th></tr>
          </thead>
          <tbody>
            @for (entry of d.recentChanges; track entry.id) {
              <tr>
                <td>{{ when(entry.occurredAtUtc) }}</td>
                <td>{{ entry.userName ?? 'სისტემა' }}</td>
                <td>{{ entry.entityType }}</td>
                <td>{{ action(entry.action) }}</td>
              </tr>
            } @empty {
              <tr><td colspan="4" class="adm-empty">ცვლილებები ჯერ არ არის.</td></tr>
            }
          </tbody>
        </table>
      </section>

      <section class="adm-card adm-help">
        <h2 class="adm-card__title">სწრაფი ინსტრუქცია</h2>
        <ol>
          <li><b>სიახლე:</b> „სიახლეები“ → „+ ახალი“ → შეავსეთ სათაური, ანოტაცია, ტექსტი და სურათი → „შენახვა“ → „გამოქვეყნება“ (ან მიუთითეთ მომავალი თარიღი).</li>
          <li><b>STEM შაბათობა:</b> შექმენით ანონსი (თარიღი თბილისის დროით, ფორმატი, მომხსენებლები) → „გამოქვეყნება“. შეხვედრის შემდეგ → „ჩატარებულად მონიშვნა“ და დაამატეთ შეჯამება, ფოტოები, ჩანაწერი და მასალები — გვერდი გადავა არქივში იმავე მისამართზე.</li>
          <li><b>პროფილი:</b> „ადამიანები“ → „+ ახალი“ → დაამატეთ როლები (გუნდი, მკვლევარი, მენტორი…). ერთ ადამიანს შეიძლება ჰქონდეს რამდენიმე როლი ერთ პროფილზე. წასულ წევრს მიუთითეთ როლის დასრულების თარიღი — ის დარჩება არქივში.</li>
        </ol>
      </section>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardPage {
  private readonly api = inject(AdminApi);
  private readonly project = inject(AdminProject);
  protected readonly data = signal<Dashboard | null>(null);
  protected readonly problem = signal<ProblemView | null>(null);

  constructor() {
    effect(() => {
      this.project.slug();
      untracked(() =>
        this.api.dashboard().subscribe({ next: (d) => this.data.set(d), error: (e: unknown) => this.problem.set(toProblem(e)) }),
      );
    });
  }

  protected when(utc: string): string {
    return formatUtc(utc);
  }

  protected bytes(value: number): string {
    return formatBytes(value);
  }

  protected action(value: string): string {
    return actions[value] ?? value;
  }
}
