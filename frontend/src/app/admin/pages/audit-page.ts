import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { AuditEntry } from '../core/admin.models';
import { ListPage } from '../core/list-page';
import { Pager, ProblemBox } from '../ui/widgets';

const actions: Readonly<Record<string, string>> = { Created: 'შეიქმნა', Updated: 'შეიცვალა', Deleted: 'წაიშალა' };

@Component({
  selector: 'adm-audit-page',
  imports: [FormsModule, Pager, ProblemBox],
  template: `
    <header class="adm-page-head"><h1>ცვლილებების ისტორია</h1></header>
    <p class="adm-lead">ვინ, რა და როდის შეცვალა. ჩანაწერები არ იშლება.</p>
    <div class="adm-toolbar">
      <input type="text" placeholder="ობიექტის ტიპი (მაგ. NewsArticle, Event, Person)" [(ngModel)]="entityType" (keyup.enter)="reload()" />
      <input type="text" placeholder="ობიექტის ID" [(ngModel)]="entityId" (keyup.enter)="reload()" />
      <button type="button" class="adm-btn" (click)="reload()">ფილტრი</button>
    </div>
    <adm-problem [problem]="problem()" />
    <div class="adm-table-wrap">
      <table class="adm-table">
        <thead><tr><th>დრო</th><th>ვინ</th><th>ობიექტი</th><th>მოქმედება</th><th>ცვლილებები</th></tr></thead>
        <tbody>
          @for (e of result()?.items; track e.id) {
            <tr>
              <td style="white-space: nowrap">{{ when(e.occurredAtUtc) }}</td>
              <td>{{ e.userName ?? 'სისტემა' }}</td>
              <td>{{ e.entityType }}<small class="adm-hint"><code>{{ e.entityId }}</code></small></td>
              <td>{{ action(e.action) }}</td>
              <td>
                @if (e.changesJson) {
                  @if (open() === e.id) {
                    <pre class="adm-json">{{ pretty(e.changesJson) }}</pre>
                    <button type="button" class="adm-btn adm-btn--sm adm-btn--ghost" (click)="open.set(null)">დამალვა</button>
                  } @else {
                    <button type="button" class="adm-btn adm-btn--sm" (click)="open.set(e.id)">ნახვა</button>
                  }
                } @else { — }
              </td>
            </tr>
          } @empty {
            <tr><td colspan="5" class="adm-empty">{{ loading() ? 'იტვირთება…' : 'ჩანაწერები ვერ მოიძებნა.' }}</td></tr>
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
export class AuditPage extends ListPage<AuditEntry> {
  protected entityType = '';
  protected entityId = '';
  protected readonly open = signal<number | null>(null);

  protected fetch(page: number) {
    return this.api.audit({ page, pageSize: 50, entityType: this.entityType.trim(), entityId: this.entityId.trim() });
  }

  protected action(value: string): string {
    return actions[value] ?? value;
  }

  protected pretty(json: string): string {
    try {
      return JSON.stringify(JSON.parse(json), null, 2);
    } catch {
      return json;
    }
  }
}
