import { ChangeDetectionStrategy, Component, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { LText } from '../core/admin.models';
import { RichEditor } from './rich-editor';

/**
 * Georgian + English value. Georgian is the main language; English is optional and the public
 * site falls back to Georgian when it is empty. The bound object is edited in place.
 */
@Component({
  selector: 'adm-ltext',
  imports: [FormsModule, RichEditor],
  template: `
    <div class="adm-field">
      <div class="adm-field__label">
        {{ label() }}
        @if (required()) {
          <span class="adm-req" aria-hidden="true">*</span>
        }
        @if (kind() === 'rich') {
          <span class="adm-lang-tabs" role="tablist">
            <button type="button" role="tab" [attr.aria-selected]="lang() === 'ka'" [class.is-on]="lang() === 'ka'" (click)="lang.set('ka')">ქართ.</button>
            <button type="button" role="tab" [attr.aria-selected]="lang() === 'en'" [class.is-on]="lang() === 'en'" (click)="lang.set('en')">EN</button>
          </span>
        }
      </div>
      @switch (kind()) {
        @case ('rich') {
          @if (lang() === 'ka') {
            <adm-rich-editor [(html)]="value().ka" />
          } @else {
            <adm-rich-editor [(html)]="value().en" />
          }
        }
        @case ('textarea') {
          <div class="adm-ltext">
            <span class="adm-ltext__lang">KA</span>
            <textarea [rows]="rows()" [(ngModel)]="value().ka" [attr.maxlength]="maxLength()" [attr.aria-label]="label() + ' (ქართული)'"></textarea>
            <span class="adm-ltext__lang">EN</span>
            <textarea [rows]="rows()" [(ngModel)]="value().en" [attr.maxlength]="maxLength()" [attr.aria-label]="label() + ' (English)'"></textarea>
          </div>
        }
        @default {
          <div class="adm-ltext">
            <span class="adm-ltext__lang">KA</span>
            <input type="text" [(ngModel)]="value().ka" [attr.maxlength]="maxLength()" [attr.aria-label]="label() + ' (ქართული)'" />
            <span class="adm-ltext__lang">EN</span>
            <input type="text" [(ngModel)]="value().en" [attr.maxlength]="maxLength()" [attr.aria-label]="label() + ' (English)'" />
          </div>
        }
      }
      @if (hint()) {
        <p class="adm-hint">{{ hint() }}</p>
      }
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.Default,
})
export class LTextField {
  readonly label = input.required<string>();
  readonly value = input.required<LText>();
  readonly kind = input<'text' | 'textarea' | 'rich'>('text');
  readonly required = input(false);
  readonly hint = input<string | null>(null);
  readonly rows = input(3);
  readonly maxLength = input<number | null>(null);

  protected readonly lang = signal<'ka' | 'en'>('ka');
}
