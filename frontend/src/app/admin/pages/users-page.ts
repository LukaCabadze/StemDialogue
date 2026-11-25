import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';

import { AdminApi } from '../core/admin-api.service';
import { AdminAuth } from '../core/admin-auth.service';
import { UserAdmin, UserRole } from '../core/admin.models';
import { formatUtc } from '../core/admin-time';
import { ProblemView, toProblem } from '../core/problem';
import { Toasts } from '../core/toasts.service';
import { ProblemBox } from '../ui/widgets';

interface UserDraft {
  email: string;
  fullName: string;
  role: UserRole;
  password: string;
}

interface UserEdit {
  id: string;
  fullName: string;
  role: UserRole;
  isActive: boolean;
}

const PASSWORD_HINT = 'მინ. 12 სიმბოლო, ლათინური ასოები (მათ შორის პატარა) და ციფრები.';

@Component({
  selector: 'adm-users-page',
  imports: [FormsModule, ProblemBox],
  template: `
    <header class="adm-page-head">
      <h1>მომხმარებლები</h1>
      <div class="adm-page-head__actions">
        <button type="button" class="adm-btn adm-btn--primary" (click)="draft.set({ email: '', fullName: '', role: 'Editor', password: '' })">+ ახალი მომხმარებელი</button>
      </div>
    </header>
    <p class="adm-lead">
      <b>ადმინისტრატორი</b> — ყველაფერი, მათ შორის დაფინანსების ტექსტი, პარტნიორები და მომხმარებლები.
      <b>რედაქტორი</b> — სიახლეები, ღონისძიებები, ადამიანები, რესურსები, გვერდები და მედია.
    </p>
    <adm-problem [problem]="problem()" />

    @if (draft(); as d) {
      <section class="adm-card">
        <h2 class="adm-card__title">ახალი მომხმარებელი</h2>
        <div class="adm-grid adm-grid--2">
          <label class="adm-field"><span class="adm-field__label">სახელი, გვარი</span><input type="text" [(ngModel)]="d.fullName" /></label>
          <label class="adm-field"><span class="adm-field__label">ელფოსტა</span><input type="email" [(ngModel)]="d.email" autocomplete="off" /></label>
          <label class="adm-field">
            <span class="adm-field__label">როლი</span>
            <select [(ngModel)]="d.role"><option value="Editor">რედაქტორი</option><option value="Admin">ადმინისტრატორი</option></select>
          </label>
          <label class="adm-field">
            <span class="adm-field__label">დროებითი პაროლი</span>
            <input type="text" [(ngModel)]="d.password" autocomplete="new-password" />
            <span class="adm-hint">{{ hint }} გადაეცით უსაფრთხოდ; მომხმარებელმა უნდა შეცვალოს.</span>
          </label>
        </div>
        <div class="adm-page-head__actions">
          <button type="button" class="adm-btn adm-btn--primary" [disabled]="busy()" (click)="create(d)">შექმნა</button>
          <button type="button" class="adm-btn adm-btn--ghost" (click)="draft.set(null)">გაუქმება</button>
        </div>
      </section>
    }

    <div class="adm-table-wrap">
      <table class="adm-table">
        <thead><tr><th>სახელი</th><th>როლი</th><th>სტატუსი</th><th>ბოლო შესვლა</th><th></th></tr></thead>
        <tbody>
          @for (u of users(); track u.id) {
            @if (editing()?.id === u.id) {
              <tr>
                <td colspan="5">
                  @if (editing(); as e) {
                    <div class="adm-row__line">
                      <input type="text" [(ngModel)]="e.fullName" aria-label="სახელი" />
                      <select [(ngModel)]="e.role" aria-label="როლი" [disabled]="u.id === auth.user()?.id">
                        <option value="Editor">რედაქტორი</option><option value="Admin">ადმინისტრატორი</option>
                      </select>
                      <label class="adm-check"><input type="checkbox" [(ngModel)]="e.isActive" [disabled]="u.id === auth.user()?.id" /><span>აქტიური</span></label>
                      <button type="button" class="adm-btn adm-btn--primary adm-btn--sm" [disabled]="busy()" (click)="update(e)">შენახვა</button>
                      <button type="button" class="adm-btn adm-btn--ghost adm-btn--sm" (click)="editing.set(null)">გაუქმება</button>
                    </div>
                  }
                </td>
              </tr>
            } @else {
              <tr>
                <td><b>{{ u.fullName }}</b><small class="adm-hint">{{ u.email }}</small></td>
                <td>{{ u.role === 'Admin' ? 'ადმინისტრატორი' : 'რედაქტორი' }}</td>
                <td>
                  @if (!u.isActive) {<span class="adm-badge" data-tone="muted">გათიშული</span>}
                  @else if (u.isLockedOut) {<span class="adm-badge" data-tone="danger">დაბლოკილი</span>}
                  @else {<span class="adm-badge" data-tone="ok">აქტიური</span>}
                </td>
                <td>{{ when(u.lastLoginAtUtc) }}</td>
                <td style="white-space: nowrap">
                  <button type="button" class="adm-btn adm-btn--sm" (click)="editing.set({ id: u.id, fullName: u.fullName, role: u.role, isActive: u.isActive })">შეცვლა</button>
                  <button type="button" class="adm-btn adm-btn--sm" (click)="reset(u)">პაროლის აღდგენა</button>
                  @if (u.isLockedOut) {
                    <button type="button" class="adm-btn adm-btn--sm" (click)="unlock(u)">განბლოკვა</button>
                  }
                </td>
              </tr>
            }
          }
        </tbody>
      </table>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.Default,
})
export class UsersPage {
  private readonly api = inject(AdminApi);
  private readonly toasts = inject(Toasts);
  protected readonly auth = inject(AdminAuth);

  protected readonly users = signal<UserAdmin[]>([]);
  protected readonly draft = signal<UserDraft | null>(null);
  protected readonly editing = signal<UserEdit | null>(null);
  protected readonly busy = signal(false);
  protected readonly problem = signal<ProblemView | null>(null);
  protected readonly hint = PASSWORD_HINT;

  constructor() {
    this.load();
  }

  protected create(d: UserDraft): void {
    this.run(this.api.createUser({ ...d, email: d.email.trim() }), 'მომხმარებელი შეიქმნა', () => this.draft.set(null));
  }

  protected update(e: UserEdit): void {
    this.run(this.api.updateUser(e.id, { fullName: e.fullName, role: e.role, isActive: e.isActive }), 'შენახულია', () => this.editing.set(null));
  }

  protected reset(u: UserAdmin): void {
    const password = prompt(`ახალი დროებითი პაროლი — ${u.fullName}\n${PASSWORD_HINT}`);
    if (password) {
      this.run(this.api.resetPassword(u.id, password), 'პაროლი შეიცვალა. მომხმარებლის სესიები დასრულდა.');
    }
  }

  protected unlock(u: UserAdmin): void {
    this.run(this.api.unlockUser(u.id), 'განბლოკილია');
  }

  protected when(utc: string | null): string {
    return utc ? formatUtc(utc) : 'არასოდეს';
  }

  private load(): void {
    this.api.users().subscribe({ next: (users) => this.users.set(users), error: (e: unknown) => this.problem.set(toProblem(e)) });
  }

  private run(call: Observable<unknown>, message: string, done?: () => void): void {
    this.busy.set(true);
    this.problem.set(null);
    call.subscribe({
      next: () => {
        this.busy.set(false);
        this.toasts.success(message);
        done?.();
        this.load();
      },
      error: (error: unknown) => {
        this.busy.set(false);
        const p = toProblem(error);
        this.problem.set(p);
        this.toasts.error(p.title);
      },
    });
  }
}
