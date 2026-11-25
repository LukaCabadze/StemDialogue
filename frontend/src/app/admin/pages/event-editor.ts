import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { AdmUrlPipe } from '../core/admin-project.service';

import { EventAdmin, LText, MediaRef, lt } from '../core/admin.models';
import { formatUtc, orNull, parseUtc, tbilisiInputToUtc, utcToTbilisiInput } from '../core/admin-time';
import { EditorPage } from '../core/editor-page';
import { LTextField } from '../ui/ltext-field';
import { GalleryField, MediaField } from '../ui/media-field';
import { CheckList, Collapsible, ProblemBox, StatusBadge } from '../ui/widgets';

interface Participant {
  personId: string;
  role: string;
}

interface EventForm {
  type: string;
  title: LText;
  slug: string;
  summary: LText;
  description: LText;
  startsAt: string;
  endsAt: string;
  format: string;
  venue: LText;
  streamUrl: string;
  registrationUrl: string;
  registrationDeadline: string;
  coverImage: MediaRef | null;
  participants: Participant[];
  tagIds: string[];
  disciplineIds: string[];
  isFeatured: boolean;
  recap: LText;
  recordingUrl: string;
  gallery: MediaRef[];
  seoTitle: LText;
  seoDescription: LText;
  shareImage: MediaRef | null;
}

@Component({
  selector: 'adm-event-editor',
  imports: [FormsModule, RouterLink, LTextField, MediaField, GalleryField, CheckList, Collapsible, ProblemBox, StatusBadge, AdmUrlPipe],
  template: `
    <header class="adm-page-head">
      <div>
        <a class="adm-back" [routerLink]="'events' | admUrl">← ღონისძიებები</a>
        <h1>{{ isNew() ? 'ახალი ღონისძიება' : (item()?.title?.ka ?? '…') }}</h1>
        @if (item(); as e) {
          <p class="adm-meta">
            <adm-status [value]="e.status" />
            <adm-status [value]="e.publication" />
            <span>ბოლოს შეცვალა {{ e.audit.updatedBy ?? '—' }}, {{ when(e.audit.updatedAtUtc) }}</span>
            @if (e.publication === 'Published') {
              <a [href]="e.publicPath" target="_blank" rel="noopener">საიტზე ნახვა ↗</a>
            }
          </p>
        }
      </div>
    </header>

    <adm-problem [problem]="problem()" />

    @if (item(); as e) {
      @if (awaitingCompletion(e)) {
        <div class="adm-alert adm-alert--warn">
          <strong>ღონისძიების დრო გავიდა.</strong> თუ შეხვედრა ჩატარდა, დაამატეთ შეჯამება, ფოტოები და ჩანაწერი ქვემოთ და დააჭირეთ „ჩატარებულად მონიშვნას“.
        </div>
      }
      @if (e.status === 'Cancelled' || e.status === 'Postponed') {
        <div class="adm-alert adm-alert--info">
          <strong>{{ e.status === 'Cancelled' ? 'გაუქმებულია' : 'გადატანილია' }}.</strong>
          @if (e.statusNote.ka) { {{ e.statusNote.ka }} }
          @if (e.originalStartsAtUtc) { <p>თავდაპირველი თარიღი: {{ when(e.originalStartsAtUtc) }}</p> }
          <p>გვერდი და ბმული საიტზე რჩება — სტუმრები ხედავენ სტატუსს.</p>
        </div>
      }
    }

    @if (form(); as f) {
      <div class="adm-editor">
        <div class="adm-editor__main">
          <section class="adm-card">
            <h2 class="adm-card__title">ანონსი</h2>
            <adm-ltext label="სათაური" [value]="f.title" [required]="true" [maxLength]="200" />
            <adm-ltext label="მოკლე აღწერა (თემა)" kind="textarea" [value]="f.summary" [required]="true" [maxLength]="500" />
            <adm-ltext label="დეტალური აღწერა" kind="rich" [value]="f.description" />
            <div class="adm-grid adm-grid--2">
              <label class="adm-field">
                <span class="adm-field__label">დაწყება (თბილისის დრო) <span class="adm-req">*</span></span>
                <input type="datetime-local" [(ngModel)]="f.startsAt" />
                @if (!isNew()) {
                  <span class="adm-hint">შეცდომის გასწორება აქ. რეალური გადატანისთვის გამოიყენეთ „გადატანა“ მარჯვნივ.</span>
                }
              </label>
              <label class="adm-field">
                <span class="adm-field__label">დასრულება (თბილისის დრო)</span>
                <input type="datetime-local" [(ngModel)]="f.endsAt" />
              </label>
            </div>
            <div class="adm-grid adm-grid--2">
              <label class="adm-field">
                <span class="adm-field__label">ფორმატი</span>
                <select [(ngModel)]="f.format">
                  @for (o of lookups.value().eventFormats; track o.value) {
                    <option [value]="o.value">{{ o.label }}</option>
                  }
                </select>
              </label>
              <label class="adm-field">
                <span class="adm-field__label">ტიპი</span>
                <select [(ngModel)]="f.type">
                  @for (o of lookups.value().eventTypes; track o.value) {
                    <option [value]="o.value">{{ o.label }}</option>
                  }
                </select>
              </label>
            </div>
            @if (f.format !== 'Online') {
              <adm-ltext label="ადგილი / მისამართი" [value]="f.venue" [maxLength]="300" />
            }
            <div class="adm-grid adm-grid--2">
              @if (f.format !== 'InPerson') {
                <label class="adm-field">
                  <span class="adm-field__label">ონლაინ ტრანსლაციის ბმული</span>
                  <input type="url" [(ngModel)]="f.streamUrl" placeholder="https://" />
                </label>
              }
              <label class="adm-field">
                <span class="adm-field__label">რეგისტრაციის ბმული</span>
                <input type="url" [(ngModel)]="f.registrationUrl" placeholder="https://forms…" />
              </label>
              <label class="adm-field">
                <span class="adm-field__label">რეგისტრაციის ბოლო ვადა (თბილისი)</span>
                <input type="datetime-local" [(ngModel)]="f.registrationDeadline" />
              </label>
            </div>
            <adm-media-field label="სურათი / პოსტერი" [(media)]="f.coverImage" />
          </section>

          <section class="adm-card">
            <h2 class="adm-card__title">მომხსენებლები და მონაწილეები</h2>
            <p class="adm-hint">ღონისძიება გამოჩნდება თითოეული ადამიანის პროფილზე.</p>
            <div class="adm-rows">
              @for (p of f.participants; track $index; let i = $index, first = $first, last = $last) {
                <div class="adm-row">
                  <div class="adm-row__line">
                    <select [(ngModel)]="p.personId" aria-label="ადამიანი">
                      <option value="" disabled>— აირჩიეთ ადამიანი —</option>
                      @for (person of lookups.value().people; track person.id) {
                        <option [value]="person.id">{{ person.label }}</option>
                      }
                    </select>
                    <select [(ngModel)]="p.role" aria-label="როლი">
                      @for (r of lookups.value().participantRoles; track r.value) {
                        <option [value]="r.value">{{ r.label }}</option>
                      }
                    </select>
                    <button type="button" class="adm-icon-btn" [disabled]="first" aria-label="ზემოთ" (click)="move(f.participants, i, -1)">↑</button>
                    <button type="button" class="adm-icon-btn" [disabled]="last" aria-label="ქვემოთ" (click)="move(f.participants, i, 1)">↓</button>
                    <button type="button" class="adm-icon-btn adm-icon-btn--danger" aria-label="მოხსნა" (click)="f.participants.splice(i, 1)">✕</button>
                  </div>
                </div>
              }
            </div>
            <button type="button" class="adm-btn adm-btn--sm" (click)="f.participants.push({ personId: '', role: 'Speaker' })">+ მონაწილის დამატება</button>
            <a class="adm-btn adm-btn--sm adm-btn--ghost" [routerLink]="'people/new' | admUrl" target="_blank">ახალი პროფილის შექმნა ↗</a>
          </section>

          <section class="adm-card">
            <h2 class="adm-card__title">არქივი — შეხვედრის შემდეგ</h2>
            <p class="adm-hint">ივსება ჩატარების შემდეგ; იმავე გვერდზე გამოჩნდება როგორც შეჯამება.</p>
            <adm-ltext label="შეჯამება" kind="rich" [value]="f.recap" />
            <label class="adm-field">
              <span class="adm-field__label">ვიდეო ჩანაწერის ბმული (YouTube / Vimeo)</span>
              <input type="url" [(ngModel)]="f.recordingUrl" placeholder="https://www.youtube.com/watch?v=…" />
            </label>
            <adm-gallery-field label="ფოტოები" [(items)]="f.gallery" />
            @if (item(); as e) {
              <p class="adm-hint">პრეზენტაციები და მასალები დაამატეთ <a [routerLink]="'resources/new' | admUrl">რესურსებში</a> და მიაბით ამ ღონისძიებას.</p>
            }
          </section>

          <adm-collapsible title="SEO და გაზიარება (არასავალდებულო)">
            <adm-ltext label="SEO სათაური" [value]="f.seoTitle" [maxLength]="70" />
            <adm-ltext label="SEO აღწერა" kind="textarea" [value]="f.seoDescription" [maxLength]="200" />
            <adm-media-field label="გაზიარების სურათი" [(media)]="f.shareImage" />
          </adm-collapsible>
        </div>

        <aside class="adm-editor__side">
          <section class="adm-card">
            <h2 class="adm-card__title">სტატუსი და გამოქვეყნება</h2>
            @if (item(); as e) {
              @if (e.publication !== 'Published') {
                <button type="button" class="adm-btn adm-btn--primary adm-btn--block" [disabled]="busy()" (click)="publish()">გამოქვეყნება</button>
              } @else {
                <button type="button" class="adm-btn adm-btn--block" [disabled]="busy()" (click)="unpublish()">საიტიდან მოხსნა (დრაფტი)</button>
              }

              @if (e.status === 'Planned' || e.status === 'Postponed') {
                @if (hasStarted(e)) {
                  <button type="button" class="adm-btn adm-btn--block" [disabled]="busy()" (click)="complete()">✓ ჩატარებულად მონიშვნა</button>
                } @else {
                  <p class="adm-hint">„ჩატარებულად მონიშვნა“ გამოჩნდება ღონისძიების დაწყების შემდეგ.</p>
                }
                <button type="button" class="adm-btn adm-btn--block" [disabled]="busy()" (click)="panel.set(panel() === 'postpone' ? null : 'postpone')">გადატანა…</button>
                <button type="button" class="adm-btn adm-btn--danger adm-btn--block" [disabled]="busy()" (click)="panel.set(panel() === 'cancel' ? null : 'cancel')">გაუქმება…</button>
              }
              @if (e.status === 'Cancelled') {
                <button type="button" class="adm-btn adm-btn--block" [disabled]="busy()" (click)="restore()">გაუქმების გაუქმება</button>
              }

              @if (panel() === 'postpone') {
                <div class="adm-row">
                  <label class="adm-row__label">ახალი დაწყება (თბილისი)<input type="datetime-local" [(ngModel)]="newStart" /></label>
                  <label class="adm-row__label">ახალი დასრულება<input type="datetime-local" [(ngModel)]="newEnd" /></label>
                  <adm-ltext label="შეტყობინება სტუმრებისთვის" kind="textarea" [rows]="2" [value]="note" [required]="true" />
                  <button type="button" class="adm-btn adm-btn--primary" [disabled]="busy() || !newStart || !note.ka" (click)="postpone()">გადატანის დადასტურება</button>
                </div>
              }
              @if (panel() === 'cancel') {
                <div class="adm-row">
                  <adm-ltext label="მიზეზი / შეტყობინება" kind="textarea" [rows]="2" [value]="note" [required]="true" />
                  <button type="button" class="adm-btn adm-btn--danger" [disabled]="busy() || !note.ka" (click)="cancel()">გაუქმების დადასტურება</button>
                </div>
              }

              @if (!e.firstPublishedAtUtc) {
                <button type="button" class="adm-btn adm-btn--danger adm-btn--block" [disabled]="busy()" (click)="remove()">დრაფტის წაშლა</button>
              }
            } @else {
              <p class="adm-hint">ჯერ შეინახეთ, შემდეგ გამოაქვეყნეთ ანონსი.</p>
            }
          </section>

          <section class="adm-card">
            <label class="adm-check">
              <input type="checkbox" [(ngModel)]="f.isFeatured" />
              <span>მთავარ გვერდზე გამორჩეული</span>
            </label>
            <adm-check-list label="მიმართულებები" [options]="lookups.value().disciplines" [(selected)]="f.disciplineIds" />
            <adm-check-list label="თეგები" [options]="lookups.value().tags" [(selected)]="f.tagIds" />
            <label class="adm-field">
              <span class="adm-field__label">Slug (URL)</span>
              <input type="text" [(ngModel)]="f.slug" placeholder="ავტომატურად სათაურიდან" />
              <span class="adm-hint">შეცვლისას ძველი ბმული გადამისამართდება.</span>
            </label>
          </section>
        </aside>
      </div>

      <div class="adm-savebar">
        <span class="adm-hint">{{ dirty() ? 'არის შეუნახავი ცვლილებები' : 'ყველა ცვლილება შენახულია' }}</span>
        <button type="button" class="adm-btn adm-btn--primary" [disabled]="busy() || (!dirty() && !isNew())" (click)="save()">
          {{ busy() ? 'ინახება…' : isNew() ? 'დრაფტად შენახვა' : 'შენახვა' }}
        </button>
      </div>
    } @else if (loading()) {
      <p class="adm-empty">იტვირთება…</p>
    }
  `,
  changeDetection: ChangeDetectionStrategy.Default,
})
export class EventEditorPage extends EditorPage<EventAdmin, EventForm> {
  protected readonly listPath = 'events';
  protected readonly panel = signal<'postpone' | 'cancel' | null>(null);
  protected newStart = '';
  protected newEnd = '';
  protected note: LText = lt(null);

  protected fetch(id: string) {
    return this.api.events.get(id);
  }

  protected createItem(request: object) {
    return this.api.events.create(request);
  }

  protected updateItem(id: string, request: object) {
    return this.api.events.update(id, request);
  }

  protected toForm(e: EventAdmin | null): EventForm {
    return {
      type: e?.type ?? 'StemSaturday',
      title: lt(e?.title),
      slug: e?.slug ?? '',
      summary: lt(e?.summary),
      description: lt(e?.description),
      startsAt: utcToTbilisiInput(e?.startsAtUtc) || nextSaturdayAtNoon(),
      endsAt: utcToTbilisiInput(e?.endsAtUtc),
      format: e?.format ?? 'Online',
      venue: lt(e?.venue),
      streamUrl: e?.streamUrl ?? '',
      registrationUrl: e?.registrationUrl ?? '',
      registrationDeadline: utcToTbilisiInput(e?.registrationDeadlineUtc),
      coverImage: e?.coverImage ?? null,
      participants: (e?.participants ?? []).map((p) => ({ personId: p.personId, role: p.role })),
      tagIds: [...(e?.tagIds ?? [])],
      disciplineIds: [...(e?.disciplineIds ?? [])],
      isFeatured: e?.isFeatured ?? false,
      recap: lt(e?.recap),
      recordingUrl: e?.recordingUrl ?? '',
      gallery: [...(e?.gallery ?? [])],
      seoTitle: lt(e?.seoTitle),
      seoDescription: lt(e?.seoDescription),
      shareImage: e?.shareImage ?? null,
    };
  }

  protected toRequest(f: EventForm): object {
    return {
      type: f.type,
      title: f.title,
      slug: orNull(f.slug),
      summary: f.summary,
      description: f.description,
      startsAtUtc: tbilisiInputToUtc(f.startsAt),
      endsAtUtc: tbilisiInputToUtc(f.endsAt),
      format: f.format,
      venue: f.format === 'Online' ? null : f.venue,
      streamUrl: f.format === 'InPerson' ? null : orNull(f.streamUrl),
      registrationUrl: orNull(f.registrationUrl),
      registrationDeadlineUtc: tbilisiInputToUtc(f.registrationDeadline),
      coverImageId: f.coverImage?.id ?? null,
      participants: f.participants.filter((p) => p.personId),
      tagIds: f.tagIds,
      disciplineIds: f.disciplineIds,
      isFeatured: f.isFeatured,
      recap: f.recap,
      recordingUrl: orNull(f.recordingUrl),
      galleryImageIds: f.gallery.map((m) => m.id),
      seoTitle: f.seoTitle,
      seoDescription: f.seoDescription,
      shareImageId: f.shareImage?.id ?? null,
    };
  }

  protected override afterSave(): void {
    this.lookups.reload();
    this.panel.set(null);
  }

  protected when(utc: string | null): string {
    return formatUtc(utc);
  }

  protected awaitingCompletion(e: EventAdmin): boolean {
    return (e.status === 'Planned' || e.status === 'Postponed') && parseUtc(e.endsAtUtc ?? e.startsAtUtc) < new Date();
  }

  protected hasStarted(e: EventAdmin): boolean {
    return parseUtc(e.startsAtUtc) <= new Date();
  }

  protected move(list: Participant[], index: number, delta: number): void {
    const [row] = list.splice(index, 1);
    list.splice(index + delta, 0, row);
  }

  protected publish(): void {
    this.runAction((e) => this.api.events.action(e.id, 'publish'), 'გამოქვეყნდა');
  }

  protected unpublish(): void {
    if (confirm('ღონისძიება საიტიდან მოიხსნას? ბმული დროებით აღარ იმუშავებს.')) {
      this.runAction((e) => this.api.events.action(e.id, 'unpublish'), 'მოიხსნა საიტიდან');
    }
  }

  protected complete(): void {
    if (confirm('მოინიშნოს ჩატარებულად? გვერდი გადავა არქივში იმავე მისამართზე.')) {
      this.runAction((e) => this.api.events.action(e.id, 'complete', {}), 'მოინიშნა ჩატარებულად');
    }
  }

  protected postpone(): void {
    const body = { newStartsAtUtc: tbilisiInputToUtc(this.newStart), newEndsAtUtc: tbilisiInputToUtc(this.newEnd), note: this.note };
    this.runAction((e) => this.api.events.action(e.id, 'postpone', body), 'გადატანილია');
    this.resetPanel();
  }

  protected cancel(): void {
    this.runAction((e) => this.api.events.action(e.id, 'cancel', { note: this.note }), 'გაუქმდა');
    this.resetPanel();
  }

  protected restore(): void {
    this.runAction((e) => this.api.events.action(e.id, 'restore'), 'აღდგა');
  }

  protected remove(): void {
    this.deleteItem((id) => this.api.events.remove(id), 'წავშალოთ ეს დრაფტი? მოქმედება შეუქცევადია.');
  }

  private resetPanel(): void {
    this.newStart = '';
    this.newEnd = '';
    this.note = lt(null);
  }
}

/** Default start for a new STEM Saturday: the coming Saturday, 12:00 Tbilisi time. */
function nextSaturdayAtNoon(): string {
  const now = new Date(Date.now() + 4 * 3600_000);
  const days = (6 - now.getUTCDay() + 7) % 7 || 7;
  const date = new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate() + days, 12, 0));
  return date.toISOString().slice(0, 16);
}
