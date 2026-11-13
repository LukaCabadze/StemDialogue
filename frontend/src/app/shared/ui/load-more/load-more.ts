import { ChangeDetectionStrategy, Component, inject, input, output } from '@angular/core';

import { LanguageService } from '../../../core/i18n/language.service';

/** "Show more" button under paged lists; hidden when there is nothing left to load. */
@Component({
  selector: 'sd-load-more',
  template: `
    @if (hasMore()) {
      <button type="button" class="sd-btn sd-btn--secondary" [disabled]="loading()" (click)="more.emit()">
        {{ loading() ? text().common.loading : text().common.loadMore }}
      </button>
    }
  `,
  styles: `
    :host {
      display: flex;
      justify-content: center;
      margin-top: 40px;
    }
    :host:empty {
      display: none;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoadMore {
  protected readonly text = inject(LanguageService).text;

  readonly hasMore = input.required<boolean>();
  readonly loading = input(false);
  readonly more = output<void>();
}
