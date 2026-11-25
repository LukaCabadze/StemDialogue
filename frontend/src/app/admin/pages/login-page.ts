import { ChangeDetectionStrategy, Component, ViewEncapsulation, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { AdminAuth } from '../core/admin-auth.service';
import { AdminProject } from '../core/admin-project.service';
import { toProblem } from '../core/problem';

@Component({
  selector: 'adm-login-page',
  imports: [FormsModule],
  template: `
    <div class="adm-login">
      <form class="adm-login__card" ngNoForm (submit)="$event.preventDefault(); submit()">
        <img src="brand/symbol.png" alt="" width="48" height="48" />
        <h1>STEM Dialogue</h1>
        <p class="adm-hint">კონტენტის მართვის პანელი</p>

        @if (error()) {
          <div class="adm-alert adm-alert--danger" role="alert">{{ error() }}</div>
        }

        <label class="adm-field">
          <span class="adm-field__label">ელფოსტა</span>
          <input type="email" name="email" autocomplete="username" required [(ngModel)]="email" />
        </label>
        <label class="adm-field">
          <span class="adm-field__label">პაროლი</span>
          <input type="password" name="password" autocomplete="current-password" required [(ngModel)]="password" />
        </label>
        <button type="submit" class="adm-btn adm-btn--primary adm-btn--block" [disabled]="busy() || !email || !password">
          {{ busy() ? 'შესვლა…' : 'შესვლა' }}
        </button>
        <a class="adm-login__back" [href]="project.publicUrl()">← საიტზე დაბრუნება</a>
      </form>
    </div>
  `,
  styleUrl: '../layout/admin-base.scss',
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.Default,
})
export class LoginPage {
  private readonly auth = inject(AdminAuth);
  private readonly router = inject(Router);
  protected readonly project = inject(AdminProject);

  readonly returnUrl = input<string | undefined>();
  protected email = '';
  protected password = '';
  protected readonly busy = signal(false);
  protected readonly error = signal<string | null>(null);

  protected submit(): void {
    this.busy.set(true);
    this.error.set(null);
    this.auth.login(this.email.trim(), this.password).subscribe({
      next: () => {
        this.busy.set(false);
        const target = this.returnUrl();
        const home = this.project.url();
        void this.router.navigateByUrl(target?.startsWith(home) ? target : home);
      },
      error: (error: unknown) => {
        this.busy.set(false);
        const problem = toProblem(error);
        this.error.set(problem.status === 401 ? 'ელფოსტა ან პაროლი არასწორია, ან ანგარიში დროებით დაბლოკილია.' : problem.title);
      },
    });
  }
}
