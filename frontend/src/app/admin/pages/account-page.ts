import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { AdminAuth } from '../core/admin-auth.service';
import { AdminProject } from '../core/admin-project.service';
import { ProblemView, toProblem } from '../core/problem';
import { ProblemBox } from '../ui/widgets';

@Component({
  selector: 'adm-account-page',
  imports: [FormsModule, ProblemBox],
  template: `
    <header class="adm-page-head"><h1>ჩემი ანგარიში</h1></header>
    @if (auth.user(); as u) {
      <section class="adm-card">
        <p><b>{{ u.fullName }}</b></p>
        <p class="adm-hint">{{ u.email }} · {{ auth.isAdmin() ? 'ადმინისტრატორი' : 'რედაქტორი' }}</p>
      </section>
    }
    <section class="adm-card" style="max-width: 520px">
      <h2 class="adm-card__title">პაროლის შეცვლა</h2>
      <adm-problem [problem]="problem()" />
      <label class="adm-field">
        <span class="adm-field__label">მიმდინარე პაროლი</span>
        <input type="password" autocomplete="current-password" [(ngModel)]="current" />
      </label>
      <label class="adm-field">
        <span class="adm-field__label">ახალი პაროლი</span>
        <input type="password" autocomplete="new-password" [(ngModel)]="next" />
        <span class="adm-hint">მინ. 12 სიმბოლო, ლათინური ასოები (მათ შორის პატარა) და ციფრები.</span>
      </label>
      <label class="adm-field">
        <span class="adm-field__label">გაიმეორეთ ახალი პაროლი</span>
        <input type="password" autocomplete="new-password" [(ngModel)]="repeat" />
        @if (repeat && repeat !== next) {
          <span class="adm-error-text">პაროლები არ ემთხვევა.</span>
        }
      </label>
      <button type="button" class="adm-btn adm-btn--primary" [disabled]="busy() || !current || !next || next !== repeat" (click)="change()">
        პაროლის შეცვლა
      </button>
      <p class="adm-hint" style="margin-top: 8px">შეცვლის შემდეგ ყველა სესია დასრულდება და თავიდან შეხვალთ.</p>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.Default,
})
export class AccountPage {
  protected readonly auth = inject(AdminAuth);
  private readonly router = inject(Router);
  private readonly project = inject(AdminProject);
  protected current = '';
  protected next = '';
  protected repeat = '';
  protected readonly busy = signal(false);
  protected readonly problem = signal<ProblemView | null>(null);

  protected change(): void {
    this.busy.set(true);
    this.problem.set(null);
    this.auth.changePassword(this.current, this.next).subscribe({
      next: () => void this.router.navigateByUrl(this.project.url('login')),
      error: (error: unknown) => {
        this.busy.set(false);
        this.problem.set(toProblem(error));
      },
    });
  }
}
