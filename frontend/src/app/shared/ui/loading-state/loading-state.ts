import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';

import { LanguageService } from '../../../core/i18n/language.service';

export type LoadingLayout = 'cards' | 'detail' | 'list';

/** Skeleton placeholder shown while a page's data loads. */
@Component({
  selector: 'sd-loading-state',
  templateUrl: './loading-state.html',
  styleUrl: './loading-state.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    role: 'status',
    'aria-live': 'polite',
    '[class]': "'layout-' + layout()",
  },
})
export class LoadingState {
  protected readonly text = inject(LanguageService).text;

  readonly layout = input<LoadingLayout>('cards');
  readonly count = input(3);

  protected readonly blocks = computed(() => Array.from({ length: this.count() }, (_, index) => index));
}
